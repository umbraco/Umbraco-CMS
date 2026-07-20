import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class ProfilingUiHelper extends UiBaseLocators {
  private readonly profilingTab: Locator;
  private readonly activateProfilerByDefaultToggle: Locator;
  private readonly activateProfilerByDefaultCheckbox: Locator;

  constructor(page: Page) {
    super(page);
    this.profilingTab = page.getByRole('tab', {name: 'Profiling'});
    this.activateProfilerByDefaultToggle = page.locator("[label='Activate the profiler by default'] #toggle");
    this.activateProfilerByDefaultCheckbox = page.locator("[label='Activate the profiler by default'] input[type='checkbox']");
  }

  async clickProfilingTab() {
    await this.click(this.profilingTab);
  }

  async clickActivateProfilerByDefaultToggle() {
    await this.click(this.activateProfilerByDefaultToggle);
  }

  async isActivateProfilerByDefaultToggleChecked(isChecked: boolean) {
    return expect(this.activateProfilerByDefaultCheckbox).toBeChecked({checked: isChecked});
  }
}
