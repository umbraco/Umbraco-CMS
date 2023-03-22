import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIMultiUrlPickerElement } from './property-editor-ui-multi-url-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMultiUrlPickerElement', () => {
  let element: UmbPropertyEditorUIMultiUrlPickerElement;

  beforeEach(async () => {
	element = await fixture(
	  html` <umb-property-editor-ui-multi-url-picker></umb-property-editor-ui-multi-url-picker> `
	);
  });

  it('is defined with its own instance', () => {
	expect(element).to.be.instanceOf(UmbPropertyEditorUIMultiUrlPickerElement);
  });

  it('passes the a11y audit', async () => {
	await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
