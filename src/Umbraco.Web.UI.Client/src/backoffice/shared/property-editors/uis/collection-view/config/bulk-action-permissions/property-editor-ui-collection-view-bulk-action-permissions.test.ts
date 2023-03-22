import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICollectionViewBulkActionPermissionsElement } from './property-editor-ui-collection-view-bulk-action-permissions.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionViewBulkActionPermissionsElement', () => {
  let element: UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-collection-view-bulk-action-permissions></umb-property-editor-ui-collection-view-bulk-action-permissions> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionViewBulkActionPermissionsElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
