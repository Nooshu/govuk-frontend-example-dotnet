# Hosting

This example runs as a container on [Cloud Run](https://cloud.google.com/run/docs). See [deploying-on-cloud-run.md](deploying-on-cloud-run.md).

## What the image does

[`Dockerfile`](../Dockerfile) is multi-stage:

1. **Node 22** runs `npm ci` and `node scripts/build-assets-cli.mjs` (Sass, fingerprinted CSS, `initAll()` module, fonts, and images).
2. **.NET 10 SDK** publishes the Razor Pages app with `SkipGovUkAssets=true`, because that image has no Node.
3. **ASP.NET runtime** runs `GovUk.Frontend.Example.dll`.

The runtime image also contains the pinned `govuk-frontend` package so the catalogue can read `fixtures.json`. `GOVUK_FRONTEND_ROOT` points at that copy. [`baseline/policy.json`](../baseline/policy.json) is published next to the app.

## Cloud Run

[`scripts/deploy-cloud-run.sh`](../scripts/deploy-cloud-run.sh) builds this image with Cloud Build and deploys it. Leave `PORT` unset in the Dockerfile. Cloud Run sets it. The app listens on `0.0.0.0:$PORT` and trusts `X-Forwarded-Proto` (and `X-Forwarded-For`) from the platform proxy, including unknown proxy addresses, because Cloud Run’s edge is not in Kestrel’s default known-proxy list.

With `MIN_INSTANCES=0` the service scales to zero. The next request waits while the process starts. The filesystem is ephemeral, and the fishing-rod journey stores answers in memory, so a restart clears that session.

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
