import {test as base} from "@playwright/test"
import {ApiHelpers, UiHelpers} from ".";
import {ConsoleErrorHelper} from "./ConsoleErrorHelper";

const test = base.extend<{ umbracoApi: ApiHelpers } & { umbracoUi: UiHelpers }>({
  umbracoApi: async ({page}, use) => {
    const umbracoApi = new ApiHelpers(page);
    // Runs the isLoginStateValid before each implementation of umbracoApi in our tests (Which is every single one). This makes sure that the login state is valid
    await umbracoApi.isLoginStateValid();
    await use(umbracoApi);
    // Save browser context so the next test starts with valid cookies.
    try {
      const filePath = process.env.STORAGE_STATE_PATH;
      if (filePath) {
        // Clear localStorage so preferences (e.g. selected language) don't leak between tests
        await page.evaluate(() => localStorage.clear());
        await page.context().storageState({path: filePath});
      }
    } catch {
      // Page may already be closed after a timeout.
    }
  },

  umbracoUi: async ({page}, use) => {
    const umbracoUi = new UiHelpers(page);
    const consoleErrorHelper = new ConsoleErrorHelper();

    // Listen for all console events and handle errors
    page.on('console', message => {
      if (message.type() === 'error') {
        const errorMessage = message.text();
        const testTitle = test.info().title;
        const testLocation = test.info().titlePath[0];
        let errorMessageJson = consoleErrorHelper.updateConsoleErrorTextToJson(errorMessage, testTitle, testLocation);
        consoleErrorHelper.writeConsoleErrorToFile(errorMessageJson);
      }
    });

    await use(umbracoUi);
  }
})

export {test};
