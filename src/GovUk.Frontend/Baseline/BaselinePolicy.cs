using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GovUk.Frontend.Baseline;

public sealed record PreloadLink(string Href, string As, string? Type = null, bool Crossorigin = false);

public sealed class ResponseHeaderOptions
{
    public required string Kind { get; init; }

    public required bool SecureTransport { get; init; }

    public bool HstsPreload { get; init; }

    public bool SetsCookie { get; init; }

    public string? ContentType { get; init; }

    public string? Filename { get; init; }

    public string? ReferrerPolicy { get; init; }

    public string? CrossOriginResourcePolicy { get; init; }

    public string? CrossOriginEmbedderPolicy { get; init; }

    public bool? EnforceContentSecurityPolicy { get; init; }

    public IReadOnlyList<string>? Vary { get; init; }

    public IReadOnlyDictionary<string, IReadOnlyList<string>>? CspSources { get; init; }

    public IReadOnlyList<string>? ScriptHashes { get; init; }

    public string? ScriptNonce { get; init; }

    public bool AllowUnsafeCspSources { get; init; }

    public IReadOnlyDictionary<string, string>? PermissionsAllow { get; init; }

    public IReadOnlyList<PreloadLink>? Preload { get; init; }
}

public sealed record BaselineResult(
    string Kind,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyList<string> Remove);

public sealed class BaselinePolicy
{
    private static readonly Regex NoncePattern = new("^[A-Za-z0-9+/_-]{22,}={0,2}\\z", RegexOptions.CultureInvariant);
    private static readonly Regex HashPattern = new("^sha256-[A-Za-z0-9+/]+=*\\z", RegexOptions.CultureInvariant);
    private static readonly Regex VaryToken = new("^[!#$%&'*+\\-.^_`|~0-9A-Za-z]+\\z", RegexOptions.CultureInvariant);
    private static readonly Regex PermissionOrigin = new("^https://[A-Za-z0-9.-]+(?::\\d+)?\\z", RegexOptions.CultureInvariant);
    private static readonly Regex PreloadType = new("^[\\w.+-]+/[\\w.+-]+\\z", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> DocumentKinds = new(StringComparer.Ordinal)
    {
        "document",
        "sensitive-document",
    };

    private static readonly HashSet<string> DownloadKinds = new(StringComparer.Ordinal)
    {
        "download",
        "sensitive-download",
    };

    private static readonly HashSet<string> Referrers = new(StringComparer.Ordinal)
    {
        "no-referrer",
        "same-origin",
        "origin",
        "strict-origin",
        "origin-when-cross-origin",
        "strict-origin-when-cross-origin",
    };

    private static readonly HashSet<string> CorpValues = new(StringComparer.Ordinal)
    {
        "same-origin",
        "same-site",
        "cross-origin",
    };

    private static readonly HashSet<string> CoepValues = new(StringComparer.Ordinal)
    {
        "require-corp",
        "credentialless",
        "unsafe-none",
    };

    private static readonly HashSet<string> PreloadAs = new(StringComparer.Ordinal)
    {
        "style",
        "script",
        "font",
        "image",
        "fetch",
    };

    private BaselinePolicy(
        string scriptHash,
        int hstsMaxAge,
        IReadOnlyList<KeyValuePair<string, string>> headersAll,
        IReadOnlyList<KeyValuePair<string, string>> headersDocument,
        IReadOnlyList<string> remove,
        IReadOnlyDictionary<string, string> cacheControl,
        IReadOnlyDictionary<string, string> contentTypes,
        IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>> csp,
        IReadOnlyList<string> permissions,
        int nonceMinLength)
    {
        ScriptHash = scriptHash;
        HstsMaxAge = hstsMaxAge;
        HeadersAll = headersAll;
        HeadersDocument = headersDocument;
        Remove = remove;
        CacheControl = cacheControl;
        ContentTypes = contentTypes;
        Csp = csp;
        Permissions = permissions;
        NonceMinLength = nonceMinLength;
        KnownDirectives = new HashSet<string>(csp.Select(pair => pair.Key), StringComparer.Ordinal);
        KnownPermissions = new HashSet<string>(permissions, StringComparer.Ordinal);
    }

    public string ScriptHash { get; }

    public int HstsMaxAge { get; }

    public int NonceMinLength { get; }

    public IReadOnlyList<KeyValuePair<string, string>> HeadersAll { get; }

    public IReadOnlyList<KeyValuePair<string, string>> HeadersDocument { get; }

    public IReadOnlyList<string> Remove { get; }

    public IReadOnlyDictionary<string, string> CacheControl { get; }

    public IReadOnlyDictionary<string, string> ContentTypes { get; }

    public IReadOnlyList<KeyValuePair<string, IReadOnlyList<string>>> Csp { get; }

    public IReadOnlyList<string> Permissions { get; }

    private HashSet<string> KnownDirectives { get; }

    private HashSet<string> KnownPermissions { get; }

    public static BaselinePolicy Load(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        return new BaselinePolicy(
            root.GetProperty("jsEnabledScriptHash").GetString()!,
            root.GetProperty("hsts").GetProperty("maxAge").GetInt32(),
            Pairs(root.GetProperty("headers").GetProperty("all")),
            Pairs(root.GetProperty("headers").GetProperty("document")),
            Strings(root.GetProperty("remove")),
            Map(root.GetProperty("cacheControl")),
            Map(root.GetProperty("contentTypes")),
            Directives(root.GetProperty("csp").GetProperty("directives")),
            Strings(root.GetProperty("permissionsPolicy")),
            root.GetProperty("nonce").GetProperty("minLength").GetInt32());
    }

    public string CreateNonce()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }

    public static string StrongEtag(string body) => StrongEtag(Encoding.UTF8.GetBytes(body));

    public static string StrongEtag(ReadOnlySpan<byte> body)
    {
        var digest = SHA256.HashData(body);
        return $"\"{Base64Url(digest)}\"";
    }

    public string BuildContentSecurityPolicy(ResponseHeaderOptions options)
    {
        var extras = options.CspSources;
        if (extras is not null)
        {
            foreach (var name in extras.Keys)
            {
                if (!KnownDirectives.Contains(name))
                {
                    throw new ArgumentException($"unknown CSP directive: {name}");
                }
            }
        }

        var parts = new List<string>();
        foreach (var (name, tokens) in Csp)
        {
            IReadOnlyList<string> extra = extras is not null && extras.TryGetValue(name, out var found)
                ? found
                : [];
            if (tokens.Count == 0)
            {
                if (extra.Count > 0)
                {
                    throw new ArgumentException($"{name} does not take sources");
                }

                parts.Add(name);
                continue;
            }

            var merged = new List<string>(tokens);
            foreach (var source in extra)
            {
                merged.Add(AssertCspSource(name, source, options.AllowUnsafeCspSources));
            }

            if (name == "script-src")
            {
                AppendScriptSources(merged, options);
            }

            parts.Add($"{name} {string.Join(' ', merged)}");
        }

        return string.Join("; ", parts);
    }

    public BaselineResult BuildResponseHeaders(ResponseHeaderOptions options)
    {
        if (!CacheControl.ContainsKey(options.Kind))
        {
            throw new ArgumentException($"unknown response kind: {options.Kind}");
        }

        if (options.HstsPreload && !options.SecureTransport)
        {
            throw new ArgumentException("HSTS preload requires secureTransport");
        }

        var document = DocumentKinds.Contains(options.Kind);
        var download = DownloadKinds.Contains(options.Kind);
        RejectMisplaced(options, document, download);
        if (options.SetsCookie && !document)
        {
            throw new ArgumentException("Set-Cookie belongs on HTML documents, not on this response kind");
        }

        if (download && string.IsNullOrEmpty(options.Filename))
        {
            throw new ArgumentException("download filename is required");
        }

        var headers = new Dictionary<string, string>(StringComparer.Ordinal);
        var contentType = ResolveContentType(options.Kind, options.ContentType);
        if (contentType is not null)
        {
            headers["Content-Type"] = contentType;
        }

        headers["Cache-Control"] = options.SetsCookie && options.Kind == "document"
            ? "private, no-cache"
            : CacheControl[options.Kind];
        if (download)
        {
            headers["Content-Disposition"] = ContentDisposition(options.Filename!);
        }

        foreach (var pair in HeadersAll)
        {
            headers[pair.Key] = pair.Value;
        }

        ApplyReferrer(headers, options.ReferrerPolicy);
        ApplyCorp(headers, options.CrossOriginResourcePolicy);
        if (document)
        {
            ApplyDocumentHeaders(headers, options);
        }

        if (options.SecureTransport)
        {
            var hsts = $"max-age={HstsMaxAge}; includeSubDomains";
            if (options.HstsPreload)
            {
                hsts += "; preload";
            }

            headers["Strict-Transport-Security"] = hsts;
        }

        headers["Vary"] = VaryHeader(options.Vary);
        if (options.Preload is not null)
        {
            headers["Link"] = BuildPreloadLinkHeader(options.Preload);
        }

        return new BaselineResult(options.Kind, headers, Remove);
    }

    public static string BuildPreloadLinkHeader(IReadOnlyList<PreloadLink> links)
    {
        if (links.Count == 0)
        {
            throw new ArgumentException("links must be a non-empty array");
        }

        return string.Join(", ", links.Select(FormatPreload));
    }

    public static string AppendVary(string? existing, string token)
    {
        if (!VaryToken.IsMatch(token))
        {
            throw new ArgumentException("invalid Vary token");
        }

        var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in (existing ?? "").Split(','))
        {
            var trimmed = part.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (!seen.ContainsKey(trimmed))
            {
                seen[trimmed] = trimmed;
            }
        }

        if (!seen.ContainsKey(token))
        {
            seen[token] = token;
        }

        return string.Join(", ", seen.Values);
    }

    public static string ClearSiteDataHeader(IReadOnlyList<string> types)
    {
        if (types.Count == 0)
        {
            throw new ArgumentException("Clear-Site-Data types must be a non-empty array");
        }

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "cache",
            "cookies",
            "storage",
            "executionContexts",
            "*",
        };
        return string.Join(", ", types.Select(type =>
        {
            if (!allowed.Contains(type))
            {
                throw new ArgumentException($"invalid Clear-Site-Data type: {type}");
            }

            return $"\"{type}\"";
        }));
    }

    private void ApplyDocumentHeaders(Dictionary<string, string> headers, ResponseHeaderOptions options)
    {
        foreach (var pair in HeadersDocument)
        {
            headers[pair.Key] = pair.Value;
        }

        if (options.CrossOriginEmbedderPolicy is not null)
        {
            if (!CoepValues.Contains(options.CrossOriginEmbedderPolicy))
            {
                throw new ArgumentException("invalid Cross-Origin-Embedder-Policy");
            }

            headers["Cross-Origin-Embedder-Policy"] = options.CrossOriginEmbedderPolicy;
        }

        headers["Permissions-Policy"] = PermissionsPolicy(options.PermissionsAllow);
        var name = options.EnforceContentSecurityPolicy == false
            ? "Content-Security-Policy-Report-Only"
            : "Content-Security-Policy";
        headers[name] = BuildContentSecurityPolicy(options);
    }

    private string PermissionsPolicy(IReadOnlyDictionary<string, string>? allow)
    {
        allow ??= new Dictionary<string, string>();
        foreach (var name in allow.Keys)
        {
            if (!KnownPermissions.Contains(name))
            {
                throw new ArgumentException($"unknown permissions policy feature: {name}");
            }
        }

        return string.Join(", ", Permissions.Select(name =>
        {
            if (!allow.TryGetValue(name, out var value))
            {
                return $"{name}=()";
            }

            return $"{name}={PermissionAllowList(name, value)}";
        }));
    }

    private static string PermissionAllowList(string name, string value)
    {
        if (value is "none" or "()")
        {
            return "()";
        }

        if (value == "self")
        {
            return "(self)";
        }

        if (PermissionOrigin.IsMatch(value))
        {
            return $"({value})";
        }

        throw new ArgumentException($"invalid permissions allow for {name}");
    }

    private void AppendScriptSources(List<string> merged, ResponseHeaderOptions options)
    {
        var hashes = new HashSet<string>(StringComparer.Ordinal) { $"'{ScriptHash}'" };
        foreach (var hash in options.ScriptHashes ?? [])
        {
            if (!HashPattern.IsMatch(hash))
            {
                throw new ArgumentException("invalid script hash");
            }

            hashes.Add($"'{hash}'");
        }

        foreach (var hash in hashes)
        {
            merged.Add(hash);
        }

        if (options.ScriptNonce is not null)
        {
            merged.Add($"'nonce-{ValidateNonce(options.ScriptNonce)}'");
        }
    }

    private string ValidateNonce(string nonce)
    {
        if (nonce.Length < NonceMinLength || !NoncePattern.IsMatch(nonce))
        {
            throw new ArgumentException("scriptNonce must be a cryptographically random base64 string");
        }

        return nonce;
    }

    private static string AssertCspSource(string directive, string source, bool allowUnsafe)
    {
        if (source.Length == 0 || source.Any(character => character is ' ' or ';' or ','))
        {
            throw new ArgumentException($"invalid CSP source for {directive}");
        }

        var blocked = source is "*" or "'unsafe-inline'" or "'unsafe-eval'" or "'unsafe-hashes'" or "http:"
            || source.StartsWith("http://", StringComparison.Ordinal)
            || (directive.StartsWith("script-", StringComparison.Ordinal) && source is "data:" or "blob:");
        if (blocked && !allowUnsafe)
        {
            throw new ArgumentException($"{source} is not allowed on {directive} without allowUnsafeCspSources");
        }

        return source;
    }

    private void RejectMisplaced(ResponseHeaderOptions options, bool document, bool download)
    {
        if (!document && options.ScriptNonce is not null)
        {
            throw new ArgumentException("scriptNonce applies to HTML documents");
        }

        if (!document && options.CspSources is not null)
        {
            throw new ArgumentException("cspSources applies to HTML documents");
        }

        if (!document && options.EnforceContentSecurityPolicy == false)
        {
            throw new ArgumentException("CSP report-only applies to HTML documents");
        }

        if (!document && options.PermissionsAllow is not null)
        {
            throw new ArgumentException("permissionsAllow applies to HTML documents");
        }

        if (!document && options.CrossOriginEmbedderPolicy is not null)
        {
            throw new ArgumentException("crossOriginEmbedderPolicy applies to HTML documents");
        }

        if (!download && options.Filename is not null)
        {
            throw new ArgumentException("filename applies to download responses");
        }
    }

    private string? ResolveContentType(string kind, string? contentType)
    {
        var chosen = contentType ?? (ContentTypes.TryGetValue(kind, out var fromPolicy) ? fromPolicy : null);
        if (chosen is null)
        {
            return null;
        }

        if (chosen.Length == 0 || chosen.Contains('\r', StringComparison.Ordinal) || chosen.Contains('\n', StringComparison.Ordinal))
        {
            throw new ArgumentException("invalid Content-Type");
        }

        if (chosen.Contains("charset=", StringComparison.OrdinalIgnoreCase))
        {
            return chosen;
        }

        if (chosen.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || chosen.Contains("json", StringComparison.OrdinalIgnoreCase)
            || chosen.Contains("xml", StringComparison.OrdinalIgnoreCase)
            || chosen.Contains("javascript", StringComparison.OrdinalIgnoreCase))
        {
            return $"{chosen}; charset=utf-8";
        }

        return chosen;
    }

    private static string ContentDisposition(string filename)
    {
        if (filename.Length == 0 || filename.Length > 180)
        {
            throw new ArgumentException("download filename is required");
        }

        if (filename.Contains('/', StringComparison.Ordinal)
            || filename.Contains('\\', StringComparison.Ordinal)
            || filename.Contains("..", StringComparison.Ordinal))
        {
            throw new ArgumentException("download filename must not include a path");
        }

        if (filename.Contains('\r', StringComparison.Ordinal)
            || filename.Contains('\n', StringComparison.Ordinal)
            || filename.Contains('"', StringComparison.Ordinal)
            || filename.Contains(';', StringComparison.Ordinal))
        {
            throw new ArgumentException("invalid download filename");
        }

        var ascii = new string(filename.Select(character => character is >= ' ' and <= '~' ? character : '_').ToArray());
        return $"attachment; filename=\"{ascii}\"; filename*=UTF-8''{EncodeUriComponent(filename)}";
    }

    private static void ApplyReferrer(Dictionary<string, string> headers, string? referrerPolicy)
    {
        if (referrerPolicy is null)
        {
            return;
        }

        if (!Referrers.Contains(referrerPolicy))
        {
            throw new ArgumentException("invalid Referrer-Policy");
        }

        headers["Referrer-Policy"] = referrerPolicy;
    }

    private static void ApplyCorp(Dictionary<string, string> headers, string? value)
    {
        if (value is null)
        {
            return;
        }

        if (!CorpValues.Contains(value))
        {
            throw new ArgumentException("invalid Cross-Origin-Resource-Policy");
        }

        headers["Cross-Origin-Resource-Policy"] = value;
    }

    private static string VaryHeader(IReadOnlyList<string>? extra)
    {
        var tokens = new List<string> { "Accept-Encoding" };
        if (extra is null)
        {
            return string.Join(", ", tokens);
        }

        foreach (var token in extra)
        {
            if (!VaryToken.IsMatch(token))
            {
                throw new ArgumentException("invalid Vary token");
            }

            tokens.Add(token);
        }

        return string.Join(", ", tokens);
    }

    private static string FormatPreload(PreloadLink link)
    {
        if (!link.Href.StartsWith('/') || link.Href.StartsWith("//", StringComparison.Ordinal) || link.Href.Any(character => character is ' ' or '"' or '<' or '>'))
        {
            throw new ArgumentException("preload href must be a same-origin path");
        }

        if (!PreloadAs.Contains(link.As))
        {
            throw new ArgumentException($"unsupported preload as: {link.As}");
        }

        var parameters = new List<string> { $"<{link.Href}>", "rel=preload", $"as={link.As}" };
        if (link.Type is not null)
        {
            if (!PreloadType.IsMatch(link.Type))
            {
                throw new ArgumentException("invalid preload type");
            }

            parameters.Add($"type=\"{link.Type}\"");
        }

        if (link.As == "font" || link.Crossorigin)
        {
            parameters.Add("crossorigin");
        }

        return string.Join("; ", parameters);
    }

    private static string EncodeUriComponent(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value)
        {
            if (IsUriUnreserved(character))
            {
                builder.Append(character);
                continue;
            }

            foreach (var unit in Encoding.UTF8.GetBytes(character.ToString()))
            {
                builder.Append('%').Append(unit.ToString("X2"));
            }
        }

        return builder.ToString();
    }

    private static bool IsUriUnreserved(char character) =>
        character is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '-' or '_' or '.' or '!' or '~' or '*' or '\'' or '(' or ')';

    private static string Base64Url(byte[] digest) =>
        Convert.ToBase64String(digest).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static List<KeyValuePair<string, string>> Pairs(JsonElement element) =>
        element.EnumerateObject().Select(property => new KeyValuePair<string, string>(property.Name, property.Value.GetString()!)).ToList();

    private static Dictionary<string, string> Map(JsonElement element) =>
        element.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetString()!, StringComparer.Ordinal);

    private static List<string> Strings(JsonElement element) =>
        element.EnumerateArray().Select(item => item.GetString()!).ToList();

    private static List<KeyValuePair<string, IReadOnlyList<string>>> Directives(JsonElement element) =>
        element.EnumerateObject()
            .Select(property => new KeyValuePair<string, IReadOnlyList<string>>(
                property.Name,
                property.Value.EnumerateArray().Select(item => item.GetString()!).ToList()))
            .ToList();
}
