# Umbraco Acceptance Tests

You can watch a video following these instructions [here](https://www.youtube.com/watch?v=N4hBKB0U-d8) and a longer UmbraCollab recording [here](https://www.youtube.com/watch?v=hvoI28s_fDI). Make sure to use the latest recommended contribution branch rather than v10 that's mentioned in the video.  Alternatively, follow along the instructions below.

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

### Executing single tests

If you wish to run a single test, which may be helpful when writing tests you can use the following command. As before, you need to run these tests in the 'tests/Umbraco.Tests.AcceptanceTest' folder.

    npx playwright test <testname.ts>

For example to run the Login Test,

    npx playwright test tests/DefaultConfig/Login/Login.spec.ts

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

### Documentation

For further documentation on Playwright, see the [Playwright documentation](https://playwright.dev/docs/intro).