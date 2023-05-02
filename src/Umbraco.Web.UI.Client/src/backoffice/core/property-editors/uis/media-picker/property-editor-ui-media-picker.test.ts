import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIMediaPickerElement } from './property-editor-ui-media-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMediaPickerElement', () => {
  let element: UmbPropertyEditorUIMediaPickerElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-media-picker></umb-property-editor-ui-media-picker> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIMediaPickerElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
