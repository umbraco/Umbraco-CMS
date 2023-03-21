import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockGridBlockConfigurationElement } from './property-editor-ui-block-grid-block-configuration.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockGridBlockConfigurationElement', () => {
  let element: UmbPropertyEditorUIBlockGridBlockConfigurationElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-block-grid-block-configuration></umb-property-editor-ui-block-grid-block-configuration> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockGridBlockConfigurationElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
