# Authentication

## What is this?

You can now authorize against the Management API using OpenID Connect. Most endpoints will soon require a token, albeit they are open for now.

## How does it work?

You need to authorize against the Management API using OpenID Connect if you want to access protected endpoints running on a real Umbraco instance. This will give you a token that you can use to access the API. The token is stored in local storage and will be used for all subsequent requests.

If you are running the backoffice locally, you can use the `VITE_UMBRACO_USE_MSW` environment variable to bypass the OpenID Connect flow and use mocked responses instead by setting it to `on` in the `.env.local` file.

## How to use

There are two ways to use this:

### Running directly in the Umbraco-CMS repository

1. Checkout the `v13/dev` branch of [Umbraco-CMS](https://github.com/umbraco/Umbraco-cms/tree/v13/dev)
2. Run `git submodule update --init` to initialize and pull down the backoffice repository
   1. If you are using a Git GUI client, you might need to do this manually
3. Go to src/Umbraco.Web.UI.New or switch default startup project to "Umbraco.Web.UI.New"
4. Start the backend server: `dotnet run` or run the project from your IDE
5. Access https://localhost:44339/umbraco and complete the installation of Umbraco
6. You should see the log in screen after installation
7. Log in using the credentials you provided during installation

### Running with Vite

1. Perform steps 1 to 5 from before
2. Open this file in an editor: `src/Umbraco.Web.UI.New/appsettings.Development.json`
3. Add this to the Umbraco.CMS section to override the backoffice host:

```json
"Umbraco": {
	"CMS": {
		"NewBackOffice":{
			"BackOfficeHost": "http://localhost:5173",
			"AuthorizeCallbackPathName": "/"
		},
	},
	[...]
}
```

4. Set Vite to use Umbraco API by copying the ".env" file to ".env.local" and setting the following:

```
VITE_UMBRACO_USE_MSW=off
VITE_UMBRACO_API_URL=https://localhost:44339
```

5. Start the vite server: `npm run dev` in your backoffice folder
6. Check that you are sent to the login page
7. Log in

## To test a secure endpoint

If you want to mark an endpoint as secure, you can add the `[Authorize]` attribute to the controller or action. This will require you to be logged in to access the endpoint.

## What does not work yet

- You cannot log out through the UI
  - Clear your local storage to log out for now
- If your session expires or your token is revoked, you will start getting 401 network errors, which for now only will be shown as a notification in the UI - we need to figure out how to send you back to log in
- We do not _yet_ poll to see if the token is still valid or check how long before you are logged out, so you won't be notified before trying to perfor actions that require a token
