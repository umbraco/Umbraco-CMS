import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITagsStorageTypeElement } from './property-editor-ui-tags-storage-type.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITagsStorageTypeElement', () => {
  let element: UmbPropertyEditorUITagsStorageTypeElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-tags-storage-type></umb-property-editor-ui-tags-storage-type> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUITagsStorageTypeElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
