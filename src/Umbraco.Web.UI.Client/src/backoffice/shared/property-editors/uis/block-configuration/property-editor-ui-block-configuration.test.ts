import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockConfigurationElement } from './property-editor-ui-block-configuration.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPropertyEditorUIBlockConfigurationElement', () => {
  let element: UmbPropertyEditorUIBlockConfigurationElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-block-configuration></umb-property-editor-ui-block-configuration> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockConfigurationElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
