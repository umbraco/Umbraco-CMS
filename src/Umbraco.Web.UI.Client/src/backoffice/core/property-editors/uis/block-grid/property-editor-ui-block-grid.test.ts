import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockGridElement } from './property-editor-ui-block-grid.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockGridElement', () => {
  let element: UmbPropertyEditorUIBlockGridElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-block-grid></umb-property-editor-ui-block-grid> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockGridElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
