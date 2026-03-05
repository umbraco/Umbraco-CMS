import {Page, Locator, expect} from "@playwright/test";
import {UiBaseLocators} from "./UiBaseLocators";
import {umbracoConfig} from "../umbraco.config";

export class ContentRenderUiHelper extends UiBaseLocators {
  private readonly contentRenderValue: Locator;
  private readonly dataSourceRenderValue: Locator;

  constructor(page: Page) {
    super(page);
    this.contentRenderValue = page.getByTestId('content-render-value');
    this.dataSourceRenderValue = page.getByTestId('data-source-render-value');
  }

  async navigateToRenderedContentPage(contentURL: string) {
    await this.page.goto(umbracoConfig.environment.baseUrl + contentURL);
  }

  async doesContentRenderValueContainText(text: string, isEqual: boolean = false) {
    if (isEqual) {
      await this.hasText(this.contentRenderValue, text);
    } else {
      await this.containsText(this.contentRenderValue, text);
    }
  }

  async doesContentRenderValueHaveImage(src: string, width: number, height: number) {
    const imageSrc = src + '?width=' + width.toString() + '&height=' + height.toString();
    return await expect(this.contentRenderValue.locator('img')).toHaveAttribute('src', imageSrc);
  }

  async doesContentRenderValueHaveLink(linkSrc: string) {
    return await expect(this.contentRenderValue.locator('a')).toHaveAttribute('href', linkSrc);
  }

  async doesDataSourceRenderValueHaveText(text: string) {
    await this.hasText(this.dataSourceRenderValue, text);
  }
}