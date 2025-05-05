# Umbraco Acceptance Tests

You can watch a video following these instructions [here](https://www.youtube.com/watch?v=N4hBKB0U-d8) and a longer UmbraCollab recording [here](https://www.youtube.com/watch?v=hvoI28s_fDI). Make sure to use the latest recommended `main` branch rather than v10 that's mentioned in the video.  Alternatively, follow along the instructions below.

### Prerequisites
- NodeJS 22+
- A running installed Umbraco on url: [https://localhost:44339](https://localhost:44339) (Default development port)
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

If you wish to run a single set of tests, which may be helpful when writing tests you can use the following command. As before, you need to run these tests in the 'tests/Umbraco.Tests.AcceptanceTest' folder.

    npx playwright test <testname.ts>

For example to run the Login Test:

    npx playwright test tests/DefaultConfig/Login/Login.spec.ts

To run a single test (if you have several in a file), you can use this syntax.

    npx playwright test -g "<name of test>"

For example:

    npx playwright test -g "can create content with the document link"

### Executing tests in UI Mode

If you would like to have an overview of all your test, to be able to see all the steps in the tests being executed and you would like to be able to run all of your tests one after another, and maybe only just one test. Then you should use UI Mode. As before, you need to run these commands in the 'tests/Umbraco.Tests.AcceptanceTest' folder.

    npx playwright test --ui

You can also specify which tests you want to run

    npx playwright test --ui tests/DefaultConfig

When entering UI Mode, you might only able to see the authenticate test. To fix this you will need to click on the 'Projects' in UI mode and select 'Chromium'. After you've done this. You should be able to see all your tests for the location you specified when running the command.

### Environment Configuration

The environment configuration is begin setup by the npm installation script.
This results in the creation of this file: `.env`.
This file is already added to `.gitignore` and can contain values that are different for each developer machine.

The file has the following content:
```
UMBRACO_USER_LOGIN=email for superadmin
UMBRACO_USER_PASSWORD=password for superadmin
URL=https://localhost:44339
```
You can change this if you like or run the config script to reset the values, type "npm run config" in your terminal.

### Documentation

For further documentation on Playwright, see the [Playwright documentation](https://playwright.dev/docs/intro).
