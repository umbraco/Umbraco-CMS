import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockGridStylesheetPickerElement } from './property-editor-ui-block-grid-stylesheet-picker.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockGridStylesheetPickerElement', () => {
	let element: UmbPropertyEditorUIBlockGridStylesheetPickerElement;

	beforeEach(async () => {
		element = await fixture(
			html`
				<umb-property-editor-ui-block-grid-stylesheet-picker></umb-property-editor-ui-block-grid-stylesheet-picker>
			`
		);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockGridStylesheetPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
