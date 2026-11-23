import {test as base} from "@playwright/test"
import * as fs from "fs";
import {ApiHelpers, UiHelpers} from ".";
import {ConsoleErrorHelper} from "./ConsoleErrorHelper";

const test = base.extend<{ umbracoApi: ApiHelpers } & { umbracoUi: UiHelpers }>({
  umbracoApi: async ({page}, use, testInfo) => {
    const umbracoApi = new ApiHelpers(page);
    // Keep the shared admin session valid before each umbracoApi test.
    await umbracoApi.isLoginStateValid();
    await use(umbracoApi);
    await umbracoApi.report.report(testInfo);
    // Persist a valid session for the next test. Only the cookies carry over: the back office's
    // localStorage (e.g. the chosen app language) must not, whichever page the test ended on.
    try {
      const filePath = process.env.STORAGE_STATE_PATH;
      if (filePath) {
        const state = await page.context().storageState();
        fs.writeFileSync(filePath, JSON.stringify({...state, origins: []}));
      }
    } catch {
      // Page may already be closed after a timeout.
    }
  },

  umbracoUi: async ({page}, use) => {
    const umbracoUi = new UiHelpers(page);
    const consoleErrorHelper = new ConsoleErrorHelper();
    const consoleErrors: string[] = [];

    // Listen for all console events and handle errors
    page.on('console', message => {
      if (message.type() === 'error') {
        const errorMessage = message.text();
        consoleErrors.push(errorMessage);
        const testTitle = test.info().title;
        const testLocation = test.info().titlePath[0];
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
