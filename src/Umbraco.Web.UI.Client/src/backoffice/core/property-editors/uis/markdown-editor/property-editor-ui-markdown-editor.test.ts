import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIMarkdownEditorElement } from './property-editor-ui-markdown-editor.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMarkdownEditorElement', () => {
  let element: UmbPropertyEditorUIMarkdownEditorElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-markdown-editor></umb-property-editor-ui-markdown-editor> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIMarkdownEditorElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
