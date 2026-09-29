# Hosting

This example is prepared for a container host such as [Render](https://render.com/). The repository does not create a Render account and does not deploy the site.

## What the image does

[`Dockerfile`](../Dockerfile) is multi-stage:

1. **Node 22** runs `npm ci` and `node scripts/build-assets-cli.mjs` (Sass, fingerprinted CSS, `initAll()` module, fonts, and images).
2. **.NET 10 SDK** publishes the Razor Pages app with `SkipGovUkAssets=true`, because that image has no Node.
3. **ASP.NET runtime** runs `GovUk.Frontend.Example.dll`.

The runtime image also contains the pinned `govuk-frontend` package so the catalogue can read `fixtures.json`. `GOVUK_FRONTEND_ROOT` points at that copy. [`baseline/policy.json`](../baseline/policy.json) is published next to the app.

## Render

[`render.yaml`](../render.yaml) is a Docker web service on Render’s **free** plan (`plan: free`: 512 MB RAM, 0.1 CPU). Create it with **New → Blueprint** and select this repository, or create a web service by hand and choose **Free**. Leave `PORT` unset. Render sets it. The app listens on `0.0.0.0:$PORT` and trusts `X-Forwarded-Proto` (and `X-Forwarded-For`) from the platform proxy, including unknown proxy addresses, because Render’s edge is not in Kestrel’s default known-proxy list.

A free instance spins down after 15 minutes without traffic. The next request waits while the process starts, often about a minute. The filesystem is ephemeral, and the fishing-rod journey stores answers in memory, so a spin-down or restart clears that session.

The site opens on the fishing-rod start page: `GET /` and `HEAD /` redirect to `/apply`. The component catalogue remains at `/components`.

`/health` returns `ok` as `text/plain` and uses the `sensitive-document` cache kind (no-store). Use that path as the health check.

Preview and raw-fixture routes stay enabled in Production for this demonstration. A real service should limit them to Development.

Every response sends `X-Robots-Tag: noindex, nofollow`. HTML pages repeat that in a robots meta tag. `/robots.txt` allows crawling so a search engine can read the header and drop the URL. See [frontend-security.md](frontend-security.md).

## Local container

```sh
docker build -t govuk-frontend-example-dotnet .
docker run --rm -p 8080:8080 -e PORT=8080 govuk-frontend-example-dotnet
```

Then open `http://localhost:8080/health` and `http://localhost:8080/apply`.
