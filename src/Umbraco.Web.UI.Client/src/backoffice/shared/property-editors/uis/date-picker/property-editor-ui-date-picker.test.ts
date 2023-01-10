import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIDatePickerElement } from './property-editor-ui-date-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPropertyEditorUIDatePickerElement', () => {
  let element: UmbPropertyEditorUIDatePickerElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-date-picker></umb-property-editor-ui-date-picker> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIDatePickerElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
