# Umbraco.CMS.Backoffice (Bellissima)

This is the working repository of the upcoming new Backoffice to Umbraco CMS.

[![Build and test](https://github.com/umbraco/Umbraco.CMS.Backoffice/actions/workflows/build_test.yml/badge.svg)](https://github.com/umbraco/Umbraco.CMS.Backoffice/actions/workflows/build_test.yml)
[![Storybook](https://github.com/umbraco/Umbraco.CMS.Backoffice/actions/workflows/azure-static-web-apps-ambitious-stone-0033b3603.yml/badge.svg)](https://github.com/umbraco/Umbraco.CMS.Backoffice/actions/workflows/azure-static-web-apps-ambitious-stone-0033b3603.yml)
[![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=umbraco_Umbraco.CMS.Backoffice&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=umbraco_Umbraco.CMS.Backoffice)

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

If you have a local Umbraco instance running, you can use the development environment to run against it by overriding the API URL and bypassing the mock-service-worker in the frontend client.

Create a `.env.local` file and set the following variables:

```bash
VITE_UMBRACO_API_URL=https://localhost:44339 # This will be the URL to your Umbraco instance
VITE_UMBRACO_USE_MSW=off # Indicate that you want all API calls to bypass MSW (mock-service-worker)
```

Open this file in an editor: `src/Umbraco.Web.UI/appsettings.Development.json` and add this to the `Umbraco:CMS:Security` section to override the backoffice host:

```json
"Umbraco": {
	"CMS": {
		"Security":{
			"BackOfficeHost": "http://localhost:5173",
			"AuthorizeCallbackPathName": "/oauth_complete",
			"AuthorizeCallbackLogoutPathName": "/logout",
			"AuthorizeCallbackErrorPathName": "/error",
		},
	},
}
```

Now start the vite server: `npm run dev:server` in your backoffice folder and open the http://localhost:5173 URL in your browser.

### Storybook

Storybook is also being built and deployed automatically on the Main branch, including a preview URL on each pull request. See it in action on this [Azure Static Web App](https://ambitious-stone-0033b3603.1.azurestaticapps.net/).

You can test the Storybook locally by running `npm run storybook`. This will start the Storybook server and open a browser window with the Storybook UI.

Storybook is an excellent tool to test out UI components in isolation and to document them. It is also a great way to test the responsiveness and accessibility of the components.

## Contributing

We accept contributions to this project. However be aware that we are mainly working on a private backlog, so not everything will be immediately obvious. If you want to get started on contributing, please read the [contributing guidelines](./CONTRIBUTING.md).

A list of issues can be found on the [Umbraco-CMS Issue Tracker](https://github.com/umbraco/Umbraco-CMS/issues). Many of them are marked as `community/up-for-grabs` which means they are up for grabs for anyone to work on.

## Documentation

The documentation can be found on [Umbraco Docs](https://docs.umbraco.com/umbraco-cms).
