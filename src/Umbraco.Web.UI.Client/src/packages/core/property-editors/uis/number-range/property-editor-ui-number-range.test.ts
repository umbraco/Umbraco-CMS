import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUINumberRangeElement } from './property-editor-ui-number-range.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUINumberRangeElement', () => {
  let element: UmbPropertyEditorUINumberRangeElement;

  beforeEach(async () => {
    element = await fixture(
      html` <umb-property-editor-ui-number-range></umb-property-editor-ui-number-range> `
    );
  });

  it('is defined with its own instance', () => {
    expect(element).to.be.instanceOf(UmbPropertyEditorUINumberRangeElement);
  });

  it('passes the a11y audit', async () => {
    await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
