import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIImageCropperElement } from './property-editor-ui-image-cropper.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIImageCropperElement', () => {
  let element: UmbPropertyEditorUIImageCropperElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-image-cropper></umb-property-editor-ui-image-cropper> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIImageCropperElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
