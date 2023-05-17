import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITinyMceStylesheetsConfigurationElement } from './property-editor-ui-tiny-mce-stylesheets-configuration.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceStylesheetsConfigurationElement', () => {
  let element: UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-tiny-mce-stylesheets-configuration></umb-property-editor-ui-tiny-mce-stylesheets-configuration> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceStylesheetsConfigurationElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
