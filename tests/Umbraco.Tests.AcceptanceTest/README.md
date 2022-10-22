# Umbraco Acceptance Tests

### Prerequisites
- NodeJS 16+
- A running installed Umbraco on url: [https://localhost:44331](https://localhost:44331) (Default development port)
   - Install using a `SqlServer`/`LocalDb` as the tests execute too fast for `Sqlite` to handle.

### Getting started
The tests are located in the project/folder as `Umbraco.Tests.AcceptanceTests`. Make sure you run `npm ci` & `npx playwright install` in that folder, or let your IDE do that.

The script will ask you to enter the username and password for a superadmin user of your Umbraco CMS.

### Executing tests
There are two npm scripts that can be used to execute the test:

1. `npm run test`
   - Executes the tests headless.
1. `npm run ui`
   - Executes the tests in a browser handled by a playwright application.

 In case of errors it is recommended to use `await page.pause()` so you can step through your test.

### Environment Configuration

The environment configuration is begin setup by the npm installation script.
This results in the creation of this file: `.env`.
This file is already added to `.gitignore` and can contain values that are different for each developer machine.

The file has the following content:
```
UMBRACO_USER_LOGIN=email for superadmin
UMBRACO_USER_PASSWORD=password for superadmin
URL=https://localhost:44331
```
You can change this if you like or run the config script to reset the values, type "npm run config" in your terminal.
