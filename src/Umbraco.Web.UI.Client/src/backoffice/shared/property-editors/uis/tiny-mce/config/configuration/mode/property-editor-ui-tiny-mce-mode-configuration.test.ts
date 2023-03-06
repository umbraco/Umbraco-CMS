import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITinyMceModeConfigurationElement } from './property-editor-ui-tiny-mce-mode-configuration.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPropertyEditorUITinyMceModeConfigurationElement', () => {
  let element: UmbPropertyEditorUITinyMceModeConfigurationElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-tiny-mce-mode-configuration></umb-property-editor-ui-tiny-mce-mode-configuration> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceModeConfigurationElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
