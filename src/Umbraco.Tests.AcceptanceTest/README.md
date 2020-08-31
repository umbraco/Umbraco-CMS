# Umbraco Acceptance Tests

### Prerequisites
- NodeJS 12+
- A running installed Umbraco on url: [https://localhost:44331](https://localhost:44331) (Default development port)
   - Install using a `SqlServer`/`LocalDb` as the tests execute too fast for `SqlCE` to handle.
- User information in `cypress.env.json` (See [Getting started](#getting-started))

### Getting started
The tests are located in the project/folder as `Umbraco.Tests.AcceptanceTests`. Make sure you run `npm install` in that folder, or let your IDE do that.

Next, it is important that you create a new file in the root of the project called `cypress.env.json`.
This file is already added to `.gitignore` and can contain values that are different for each developer machine.

The file needs the following content:
```
{
    "username": "<email for superadmin>",
    "password": "<password for superadmin>"
}
```
Replace the `<email for superadmin>` and `<password for superadmin>` placeholders with correct info.



### Executing tests
There are two npm scripts that can be used to execute the test:

1. `npm run test`
   - Executes the tests headless.
1. `npm run ui`
   - Executes the tests in a browser handled by a cypress application.

 In case of errors it is recommended to use the UI to debug.
