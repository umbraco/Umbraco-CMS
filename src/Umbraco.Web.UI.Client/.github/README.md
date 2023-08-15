# Umbraco.CMS.Bacoffice (Bellissima)

This is the working repository of the upcoming new Backoffice to Umbraco CMS.

[![Storybook](https://github.com/umbraco/Umbraco.CMS.Backoffice/actions/workflows/azure-static-web-apps-ambitious-stone-0033b3603.yml/badge.svg)](https://github.com/umbraco/Umbraco.CMS.Backoffice/actions/workflows/azure-static-web-apps-ambitious-stone-0033b3603.yml)

## Installation instructions

1. Run `npm install`
2. Run `npm run dev` to launch Vite in dev mode

### Environment variables

The development environment supports `.env` files, so in order to set your own make a copy
of `.env` and name it `.env.local` and set the variables you need.

As an example to show the installer instead of the login screen, set the following
in the `.env.local` file to indicate that Umbraco has not been installed:

```bash
VITE_UMBRACO_INSTALL_STATUS=must-install
```

## Environments

### Development

The development environment is the default environment and is used when running `npm run dev`. All API calls are mocked and the Umbraco backoffice is served from the `src` folder.

### Run against a local Umbraco instance

> **Note**
> Make sure you have followed the [Authentication guide](../docs/authentication.md) before continuing.

If you have a local Umbraco instance running, you can use the development environment to run against it by overriding the API URL and bypassing the mock-service-worker in the frontend client.

Create a `.env.local` file and set the following variables:

```bash
VITE_UMBRACO_API_URL=https://localhost:44339 # This will be the URL to your Umbraco instance
VITE_UMBRACO_USE_MSW=off # Indicate that you want all API calls to bypass MSW (mock-service-worker)
```

### Storybook

Storybook is also being built and deployed automatically on the Main branch, including a preview URL on each pull request. See it in action on this [Azure Static Web App](https://ambitious-stone-0033b3603.1.azurestaticapps.net/).

## Contributing

We accept contributions to this project. However be aware that we are mainly working on a private backlog, so not everyone will be immediately obvious. If you want to get started on contributing, please read the [contribute space](https://github.com/umbraco/Umbraco.CMS.Backoffice/contribute) where you will be able to find the guidelines on how to contribute as well as a list of good first issues.
