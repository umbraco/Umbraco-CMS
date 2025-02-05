# Umbraco.Web.UI.Client

This is the Umbraco Backoffice UI/Client, also known as Bellissima.

This is for you as a contributor who likes to make changes to the Backoffice UI/Client project.

## Installation

1. Make sure you have [NodeJS](https://nodejs.org/en/download) installed.
2. Run `npm install` to install the dependencies.

## Ways to run a Development Server

1. If you will be working from VS Code, there is options to run both [back-end and front-end server in debug modes](#debug-via-vs-code).

2. Or you can choose to [run each of these individually](#run-a-front-end-server-against-a-local-umbraco-instance), this can be done from the Command-line/Terminal, but requires a few lines from configuration on the back-end server.

### Debug via VS Code

If you are using VS Code, you can use the `launch.json` file to start the development server. This will also start the Umbraco instance and open the browser.

You should run the task **Backoffice Launch (Vite + .NET Core)** in the **Run and Debug** panel, which will start the Vite server and the Umbraco instance. It automatically configures Umbraco (using environment variables) to use the Vite server as the Backoffice host. This task will also open a browser window, so you can start developing right away. The first time you run this task, it will take a little longer to start the Umbraco instance, but subsequent runs will be faster. Keep an eye on the Debug Console to see when the Umbraco instance is ready and then refresh the browser.

If you want to run the Vite server only, you can run the task **Backoffice Launch Vite**, which will start the Vite server only and launch a browser.

If you have an existing Vite server running, you can run the task **Backoffice Attach Vite** to attach the debugger to the Vite server.

### Run a front-end server against a local Umbraco instance


To enable the client running on a different server, we need to correct the `appsettings.json` of your project.

For code contributions you can use the backend project of `src/Umbraco.Web.UI`.
Open this file in an editor: `src/Umbraco.Web.UI/appsettings.Development.json` and add these 4 fields to the `Umbraco > CMS > Security`:

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

This will override the backoffice host URL, enabling the Client to run from a different origin.

Then start the backend server by running the command: `dotnet run` in the `Umbraco.Web.UI` folder.

#### Run the front-end server

Now start the Vite server by running the command: `npm run dev:server` in the `Umbraco.Web.UI.Client` folder.

Finally open `http://localhost:5173` in your browser.

## Contributing

If you want to get started on contributing, please read the [contributing guidelines](/.github/contributing-backoffice.md).

A list of issues can be found on the [Umbraco-CMS Issue Tracker](https://github.com/umbraco/Umbraco-CMS/issues). Many of them are marked as `community/up-for-grabs` which means they are up for grabs for anyone to work on.

## Documentation

The documentation can be found on [Umbraco Docs](https://docs.umbraco.com/umbraco-cms).

## Advanced

### Storybook

You can test the Storybook locally by running `npm run storybook`. This will start the Storybook server and open a browser window with the Storybook UI.

Storybook is an excellent tool to test out UI components in isolation and to document them. It is also a great way to test the responsiveness and accessibility of the components.

### Front-end server configuration

When running `npm run dev` the default default environment and is used when running. In this case all API calls are mocked and the Umbraco Backoffice is served from the `src` folder.

This section describes how this can be used and how you can make your own configuration.

#### Environment variables

You can setup your own environment variables for the development front-end server, which is based on Vite.

The development environment supports `.env` files, so in order to set your own make a copy
of `.env` and name it `.env.local` and set the variables you need.

As an example to show the installer instead of the login screen, set the following
in the `.env.local` file to indicate that Umbraco has not been installed:

```bash
VITE_UMBRACO_INSTALL_STATUS=must-install
```

#### Run against a local backend server

If you like the default script `npm run dev` to not use mock server and run against a server of your own configuration, this can be done via Environment variables.

Create a `.env.local` file and set the following variables:

```bash
VITE_UMBRACO_API_URL=https://localhost:44339 # This will be the URL to your Umbraco instance
VITE_UMBRACO_USE_MSW=off # Indicate that you want all API calls to bypass MSW (mock-service-worker)
```

This example above, is identical to what happens when running `npm run dev:server`.
