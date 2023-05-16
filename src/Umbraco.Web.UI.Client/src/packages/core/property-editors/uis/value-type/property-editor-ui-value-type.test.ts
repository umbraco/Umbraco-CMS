import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIValueTypeElement } from './property-editor-ui-value-type.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIValueTypeElement', () => {
  let element: UmbPropertyEditorUIValueTypeElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-value-type></umb-property-editor-ui-value-type> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUIValueTypeElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
