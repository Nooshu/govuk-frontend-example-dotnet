# Deploying on Google Cloud Run

Host this .NET example on [Cloud Run](https://cloud.google.com/run/docs). The app binds `http://0.0.0.0:$PORT` and trusts `X-Forwarded-Proto` and `X-Forwarded-For` from the platform proxy, including unknown proxy addresses. Cloud Run’s default port is 8080.

You need a Google Cloud project with billing enabled and the [gcloud CLI](https://cloud.google.com/sdk/docs/install). Cloud Run can scale to zero. Build minutes, Artifact Registry storage, and requests above the free tier are billed.

This service’s name is `govuk-frontend-example-dotnet`, region `europe-west2` (London).

## What this repo includes

| File                                                            | Role                                              |
| --------------------------------------------------------------- | ------------------------------------------------- |
| [`Dockerfile`](../Dockerfile)                                   | Node assets, `dotnet publish`, ASP.NET runtime    |
| [`scripts/deploy-cloud-run.sh`](../scripts/deploy-cloud-run.sh) | Enables APIs and deploys from source              |
| `GET /health`                                                   | Plain `ok` for startup, readiness, and liveness   |

Image stages and local `docker run` are also described in [hosting.md](hosting.md).

## Deploy

From the repo root, with a project selected:

```sh
gcloud config set project PROJECT_ID
./scripts/deploy-cloud-run.sh
```

| Variable        | Default                          |
| --------------- | -------------------------------- |
| `PROJECT`       | gcloud’s active project          |
| `REGION`        | `europe-west2`                   |
| `SERVICE`       | `govuk-frontend-example-dotnet`  |
| `MEMORY`        | `1Gi`                            |
| `CPU`           | `1`                              |
| `MIN_INSTANCES` | `0`                              |
| `MAX_INSTANCES` | `3`                              |

`GET /` redirects to `/apply`. The catalogue stays at `/components`. Answers for the fishing-rod journey are stored in memory, so a new instance starts a new session.

### After deploy checklist

- [ ] `https://<service>-<hash>.<region>.run.app/health` returns `ok`
- [ ] `/apply` loads with GOV.UK styling
- [ ] `/components` works
- [ ] A form POST in the licence journey keeps the answers on the next page

## Run the image locally

```sh
docker build -t govuk-frontend-example-dotnet .
docker run --rm -p 8080:8080 -e PORT=8080 govuk-frontend-example-dotnet
```

Open <http://127.0.0.1:8080/health> and <http://127.0.0.1:8080/apply>.
