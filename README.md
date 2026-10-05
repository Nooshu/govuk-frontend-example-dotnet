# GOV.UK Frontend example

> [!IMPORTANT]
> You are free to fork this repository and use it for your own purposes, and to modify and maintain it as you see fit.

> [!WARNING]
> 🚨 **Example repository only**
>
> This repository was created as a demonstration and will not be actively maintained or supported. It is not an official UK government project and is not endorsed, maintained, or supported by any UK government department, the Government Digital Service (GDS), or the GOV.UK Design System team.
>
> I will not be providing ongoing maintenance, updates, security fixes, or technical support.
>
> Use this code at your own risk. You are responsible for reviewing, testing, securing, maintaining, and ensuring the suitability of the code before using it in any service or production environment. I accept no responsibility or liability for any loss, damage, security issue, service failure, or other consequence resulting from its use.
>
> This repository is released under the MIT Licence. See the [LICENSE](LICENSE) file for the full licence terms.

**C# / ASP.NET Core / .NET 10** example of a **GDS-compliant** government frontend. Razor Pages render HTML with a native C# port of **[GOV.UK Frontend](https://frontend.design-system.service.gov.uk/) 6.5.1**. **No** React, Vue, Angular, Svelte, or Blazor for UI, and **no** Node at request time. Official fixtures are the HTML contract: the C# renderers match every fixture `html`.

The site is a public demonstration. It includes a component catalogue and a **fictional** “Apply for a fishing rod licence” journey. It does not issue a real licence. Search engines are asked not to index it. See [`docs/tech-stack.md`](docs/tech-stack.md), [`docs/fishing-rod-licence.md`](docs/fishing-rod-licence.md), and [`docs/hosting.md`](docs/hosting.md).

## Priorities

Frontend web performance → frontend security → reduced maintenance → accessibility → inclusive design.

## Who should read what

| You are…            | Start here                                                                                                                                                     |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Human developer** | [`docs/onboarding.md`](docs/onboarding.md) → [`CONTRIBUTING.md`](CONTRIBUTING.md) → [`docs/`](docs/README.md)                                                  |
| **AI coding agent** | [`AGENTS.md`](AGENTS.md) → [`.cursor/skills/gds-compliant-frontend/`](.cursor/skills/gds-compliant-frontend/SKILL.md) → playbooks in [`docs/`](docs/README.md) |

How docs are split for both audiences: [`docs/documentation-structure.md`](docs/documentation-structure.md).

## Run the example

```sh
npm ci
npm start
```

Open `/`. It redirects to `/apply`, the fictional licence start page. Open `/components` for the catalogue. .NET 10 SDK is required.

## Checks

```sh
npm run verify
dotnet format GovUk.Frontend.Example.slnx --verify-no-changes
dotnet test tests/GovUk.Frontend.Tests/GovUk.Frontend.Tests.csproj --settings coverlet.library.runsettings
dotnet test tests/GovUk.Frontend.Example.Tests/GovUk.Frontend.Example.Tests.csproj --settings coverlet.example.runsettings
```

## Licence and security

- Code in this repository: [MIT License](LICENSE)
- How to report vulnerabilities: [SECURITY.md](SECURITY.md)
- GOV.UK Design System and Frontend are maintained by GDS; Crown copyright / OGL apply to GOV.UK content patterns as documented on GOV.UK.
