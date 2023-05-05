import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIMemberPickerElement } from './property-editor-ui-member-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMemberPickerElement', () => {
  let element: UmbPropertyEditorUIMemberPickerElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-member-picker></umb-property-editor-ui-member-picker> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIMemberPickerElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
