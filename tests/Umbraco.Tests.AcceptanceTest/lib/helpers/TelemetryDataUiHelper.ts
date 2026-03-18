import {Page, Locator} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";

export class TelemetryDataUiHelper extends UiBaseLocators {
  private readonly telemetryDataTab: Locator;
  private readonly telemetryDataLevelToggle: Locator;

  constructor(page: Page) {
    super(page);
    this.telemetryDataTab = page.getByRole('tab', {name: 'Telemetry Data'});
    this.telemetryDataLevelToggle = page.locator('[name="telemetryLevel"] >> input[id=input]');
  }

  async clickTelemetryDataTab() {
    await this.click(this.telemetryDataTab);
  }

  async changeTelemetryDataLevelValue(value: string) {
    await this.telemetryDataLevelToggle.fill(value);
  }

  async doesTelemetryDataLevelHaveValue(value: string) {
    await this.hasValue(this.telemetryDataLevelToggle, value);
  }
}
