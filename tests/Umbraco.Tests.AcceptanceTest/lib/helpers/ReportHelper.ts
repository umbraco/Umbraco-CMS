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
            signal: AbortSignal.timeout(5000),
            method: 'POST',
            headers: {
              'content-type': 'application/json;charset=UTF-8'
            },
            body: JSON.stringify({
              "TestName": testInfo.title,
              "CommitId": process.env.CommitId,
              "RetryNumber": testInfo.retry,
              "OS": process.env.AgentOs,
              "UmbracoVersion": umbracoVersion && !umbracoVersion.startsWith('$(') ? umbracoVersion : undefined,
              // Tags such as '@smoke' are not part of testInfo.title, so they are reported separately.
              "Tags": testInfo.tags,
              "BuildNumber": process.env.BUILD_BUILDNUMBER,
              "PipelineUrl": this.getPipelineUrl()
            })
          }
        );
      } catch (error) {
        console.warn(`Failed to report the test retry: ${error}`);
      }
    }
  }

  // Link to the test results of the current Azure DevOps run, built from predefined pipeline variables.
  private getPipelineUrl(): string | undefined {
    const collectionUri = process.env.SYSTEM_TEAMFOUNDATIONCOLLECTIONURI;
    const project = process.env.SYSTEM_TEAMPROJECT;
    const buildId = process.env.BUILD_BUILDID;
    if (!collectionUri || !project || !buildId) {
      return undefined;
    }
    return `${collectionUri}${encodeURIComponent(project)}/_build/results?buildId=${buildId}&view=ms.vss-test-web.build-test-results-tab`;
  }
}