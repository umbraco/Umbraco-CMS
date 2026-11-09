import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {ConstantHelper} from "./ConstantHelper";

export class ProfilingUiHelper extends UiBaseLocators {
  private readonly profilingTab: Locator;
  private readonly activateProfilerByDefaultToggle: Locator;
  private readonly activateProfilerByDefaultCheckbox: Locator;
  private readonly notInDebugModeMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.profilingTab = page.getByRole('tab', {name: 'Profiling'});
    this.activateProfilerByDefaultToggle = page.locator("[label='Activate the profiler by default'] #toggle");
    this.activateProfilerByDefaultCheckbox = page.locator("[label='Activate the profiler by default'] input[type='checkbox']");
    this.notInDebugModeMessage = page.getByText('Umbraco currently does not run in debug mode');
  }

  async clickProfilingTab() {
    await this.click(this.profilingTab);
  }

  async clickActivateProfilerByDefaultToggle() {
    await this.click(this.activateProfilerByDefaultToggle);
  }

  async clickActivateProfilerByDefaultToggleAndWaitForUpdate() {
    return await this.waitForResponseAfterExecutingPromise(
      ConstantHelper.apiEndpoints.profilingStatus,
      this.clickActivateProfilerByDefaultToggle(),
      ConstantHelper.statusCodes.ok,
      ConstantHelper.httpMethods.put,
    );
  }

  async isActivateProfilerByDefaultToggleChecked(isChecked: boolean) {
    return expect(this.activateProfilerByDefaultCheckbox).toBeChecked({checked: isChecked});
  }

  async isActivateProfilerByDefaultToggleVisible(isVisible: boolean = true) {
    await this.isVisible(this.activateProfilerByDefaultToggle, isVisible);
  }

  async isNotInDebugModeMessageVisible(isVisible: boolean = true) {
    await this.isVisible(this.notInDebugModeMessage, isVisible);
  }
}
