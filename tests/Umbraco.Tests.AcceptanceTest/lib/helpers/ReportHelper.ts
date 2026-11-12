import {ApiHelpers} from "./ApiHelpers";
import {TestInfo} from "@playwright/test";

export class ReportHelper {
  api: ApiHelpers

  constructor(api: ApiHelpers) {
    this.api = api;
  }

  async report(testInfo: TestInfo) {
    const reportUrl = process.env.PlaywrightReportUrl;
    const umbracoVersion = process.env.UmbracoVersion;
    // An undefined pipeline variable arrives as its unexpanded macro text rather than as an empty value.
    if (testInfo.retry > 0 && process.env.CI && reportUrl && /^https?:\/\//i.test(reportUrl)) {
      // Reporting is best-effort and must never change the outcome of a test.
      try {
        await fetch(reportUrl, {
            method: 'POST',
            headers: {
              'content-type': 'application/json;charset=UTF-8'
            },
            body: JSON.stringify({
              "TestName": testInfo.title,
              "CommitId": process.env.CommitId,
              "RetryNumber": testInfo.retry,
              "OS": process.env.AgentOs,
              "UmbracoVersion": umbracoVersion && !umbracoVersion.startsWith('$(') ? umbracoVersion : undefined
            })
          }
        );
      } catch (error) {
        console.warn(`Failed to report the test retry: ${error}`);
      }
    }
  }
}