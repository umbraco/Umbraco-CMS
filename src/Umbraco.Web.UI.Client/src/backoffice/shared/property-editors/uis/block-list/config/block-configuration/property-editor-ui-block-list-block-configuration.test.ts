import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockListBlockConfigurationElement } from './property-editor-ui-block-list-block-configuration.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockListBlockConfigurationElement', () => {
  let element: UmbPropertyEditorUIBlockListBlockConfigurationElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-block-list-block-configuration></umb-property-editor-ui-block-list-block-configuration> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockListBlockConfigurationElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
