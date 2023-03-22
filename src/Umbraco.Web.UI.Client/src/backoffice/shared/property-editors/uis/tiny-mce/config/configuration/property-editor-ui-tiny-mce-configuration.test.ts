import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITinyMceConfigurationElement } from './property-editor-ui-tiny-mce-configuration.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceConfigurationElement', () => {
  let element: UmbPropertyEditorUITinyMceConfigurationElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-tiny-mce-configuration></umb-property-editor-ui-tiny-mce-configuration> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceConfigurationElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
