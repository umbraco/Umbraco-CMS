import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITreePickerElement } from './property-editor-ui-tree-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITreePickerElement', () => {
  let element: UmbPropertyEditorUITreePickerElement;

  beforeEach(async () => {
	element = await fixture(
	  html` <umb-property-editor-ui-tree-picker></umb-property-editor-ui-tree-picker> `
	);
  });

  it('is defined with its own instance', () => {
	expect(element).to.be.instanceOf(UmbPropertyEditorUITreePickerElement);
  });

  it('passes the a11y audit', async () => {
	await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
  });
});
