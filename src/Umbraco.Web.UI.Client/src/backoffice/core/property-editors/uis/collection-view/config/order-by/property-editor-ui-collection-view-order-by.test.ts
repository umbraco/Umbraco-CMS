import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICollectionViewOrderByElement } from './property-editor-ui-collection-view-order-by.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionViewOrderByElement', () => {
  let element: UmbPropertyEditorUICollectionViewOrderByElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-collection-view-order-by></umb-property-editor-ui-collection-view-order-by> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionViewOrderByElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
