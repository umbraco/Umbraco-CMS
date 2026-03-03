import {ApiHelpers} from "./ApiHelpers";
import {TestInfo} from "@playwright/test";

export class ReportHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async report(testInfo: TestInfo) {
    if (testInfo.retry > 0 && process.env.CI) {
      await fetch("https://functionapp-221110123128.azurewebsites.net/api/PlaywrightTableData", {
          method: 'POST',
          headers: {
            'content-type': 'application/json;charset=UTF-8'
          },
          body: JSON.stringify({
            "TestName": testInfo.title,
            "CommitId": process.env.CommitId,
            "RetryNumber": testInfo.retry,
            "OS": process.env.AgentOs
          })
        }
      );
    }
  }
}