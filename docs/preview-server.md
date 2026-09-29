# Preview server

The Razor Pages app is the preview server and the public catalogue.

## Run it

```sh
npm ci
npm start
```

The first build runs `node scripts/build-assets-cli.mjs`. .NET 10 SDK is required ([tech-stack.md](tech-stack.md)).

## Routes

| URL                                         | What it shows                                                                                       |
| ------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| `/`                                         | Redirects to `/apply`, the service start page.                                                      |
| `/components`                               | Catalogue: one described link per component. No fixture previews.                                   |
| `/apply`                                    | Fictional fishing-rod-licence journey. See [fishing-rod-licence.md](fishing-rod-licence.md).        |
| `/components/{name}?fixture={fixture name}` | Every fixture for that component, including hidden fixtures. Only the selected fixture is rendered. |
| `/components/{name}/raw?fixture={name}`     | The HTML fragment alone.                                                                            |
| `/health`                                   | `ok` for the host health check.                                                                     |

A component page uses the full width of the page. It shows the component’s name, a link to the Design System, the current fixture name, and a banner. The success text “The C# HTML is the same as the official fixture HTML.” is shown only when `ComponentCatalog.Render` returns the same string as that fixture’s `html`. The frame then shows that same string. The failure text is “The C# HTML is different from the official fixture HTML.” Under that, **Versions (Fixtures)** lists every fixture, including hidden ones, and marks the selected fixture with a Current tag.

The page also shows a yellow Important banner, English and Cymraeg in the service navigation, a back link to the catalogue, and the demonstration phase banner. A real service should limit these preview routes to Development.

## Where the routes run

Preview and raw-fixture routes stay on in every environment, including Production, because the hosted site is the catalogue. A real service built from this example should register them only in Development.

Responses use the same [`baseline/`](../baseline/) headers as the rest of the app. On local HTTP, HSTS is not sent. When `X-Forwarded-Proto` is `https`, HSTS is sent.

There is no syntax-highlighting asset on the layout.

## After code changes

Restart `npm start` if the process does not reload, then hard-refresh the browser. After a Frontend upgrade, check focus states, the header and footer, a form with a validation error, and a hidden fixture ([upgrading-govuk-frontend.md](upgrading-govuk-frontend.md)).
