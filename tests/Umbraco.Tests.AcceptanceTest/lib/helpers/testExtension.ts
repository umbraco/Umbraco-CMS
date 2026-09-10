import {test as base} from "@playwright/test"
import {ApiHelpers, UiHelpers} from ".";
import {ConsoleErrorHelper} from "./ConsoleErrorHelper";

const test = base.extend<{ umbracoApi: ApiHelpers } & { umbracoUi: UiHelpers }>({
  umbracoApi: async ({page}, use) => {
    const umbracoApi = new ApiHelpers(page);
    // Keep the shared admin session valid before each umbracoApi test.
    await umbracoApi.isLoginStateValid();
    await use(umbracoApi);
    // Persist a valid session for the next test. The storageState save MUST run even when the
    // localStorage clear throws (API-only test that never navigated), or a revoked session leaks on.
    try {
      const filePath = process.env.STORAGE_STATE_PATH;
      if (filePath) {
        try {
          await page.evaluate(() => localStorage.clear());
        } catch {
          // Not on an app origin (API-only test) - nothing to clear.
        }
        await page.context().storageState({path: filePath});
      }
    } catch {
      // Page may already be closed after a timeout.
    }
  },

  umbracoUi: async ({page}, use) => {
    const umbracoUi = new UiHelpers(page);
    const consoleErrorHelper = new ConsoleErrorHelper();
    const consoleErrors: string[] = [];

    // Resolved once, up front, rather than inside the handler: the listener stays attached until
    // the page closes, so it can fire during teardown - and test.info() throws when called outside
    // a running test, which inside an event listener surfaces as an unhandled error rather than a
    // test failure.
    const testTitle = test.info().title;
    const testLocation = test.info().titlePath[0];

    // Listen for all console events and handle errors
    page.on('console', message => {
      if (message.type() === 'error') {
        const errorMessage = message.text();
        consoleErrors.push(errorMessage);
        let errorMessageJson = consoleErrorHelper.updateConsoleErrorTextToJson(errorMessage, testTitle, testLocation);
        consoleErrorHelper.writeConsoleErrorToFile(errorMessageJson);
      }
    });

    await use(umbracoUi);

    // Attach console errors to the test report (de-duplicated). Non-failing by design: a visible
    // signal, not a gate, since the backoffice logs benign errors that would otherwise turn the suite red.
    if (consoleErrors.length > 0) {
      test.info().annotations.push({
        type: 'console-error',
        description: [...new Set(consoleErrors)].join('\n'),
      });
    }
  }
})

export {test};
