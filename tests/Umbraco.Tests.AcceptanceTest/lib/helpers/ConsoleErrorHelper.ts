import * as fs from "fs";

export class ConsoleErrorHelper {

  writeConsoleErrorToFile(error) {
    const filePath = process.env.CONSOLE_ERRORS_PATH;
    if (filePath) {
      try {
        const jsonString = fs.readFileSync(filePath, 'utf-8');
        const data = JSON.parse(jsonString);

        // Checks if the error already exists in the file for the specific test, and if it does. We increment the error count instead of adding it again.
        const duplicateError = data.consoleErrors.find(item => (item.testTitle === error.testTitle) && (item.errorText === error.errorText));
        if (duplicateError) {
          duplicateError.errorCount++;
          data.consoleErrors[data.consoleErrors.indexOf(duplicateError[0])] = duplicateError;
        } else {
          data.consoleErrors.push(error);
        }

        const updatedJsonString = JSON.stringify(data, null, 2);
        fs.writeFileSync(filePath, updatedJsonString, 'utf-8');
      } catch (error) {
        console.error('Error updating console error:', error);
      }
    }
  }

  updateConsoleErrorTextToJson(errorMessage: string, testTitle: string, testLocation: string) {
    return {
      testTitle: testTitle,
      testLocation: testLocation,
      errorText: errorMessage,
      errorCount: 1,
    };
  }
}