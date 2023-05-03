import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITagsElement } from './property-editor-ui-tags.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITagsElement', () => {
  let element: UmbPropertyEditorUITagsElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-tags></umb-property-editor-ui-tags> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUITagsElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
