# Umbraco.CMS.Backoffice

## Installation instructions

1. Run `npm install`
2. Run `npm run dev` to launch Vite in dev mode

### Environment variables

The development environment supports `.env` files, so in order to set your own make a copy
of `.env` and name it `.env.local` and set the variables you need.

As an example to show the installer instead of the login screen, set the following
in the `.env.local` file to indicate that Umbraco has not been installed:

```bash
VITE_UMBRACO_INSTALL_STATUS=false
```

## Environments

See the Main branch in action here as an [Azure Static Web App](https://ashy-bay-09f36a803.1.azurestaticapps.net/). The deploy runs automatically every time the `main` branch is updated. It uses mocked responses from the Umbraco API to simulate the site just like the local development environment.
