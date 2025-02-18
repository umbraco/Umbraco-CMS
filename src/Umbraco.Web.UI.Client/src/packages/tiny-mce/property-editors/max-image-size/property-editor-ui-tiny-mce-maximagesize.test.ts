/*
// Disabled because TinyMCE is not included in the test runner
import { UmbPropertyEditorUITinyMceMaxImageSizeElement } from './property-editor-ui-tiny-mce-maximagesize.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceMaxImSizeElement', () => {
	let element: UmbPropertyEditorUITinyMceMaxImageSizeElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-tiny-mce-maximagesize></umb-property-editor-ui-tiny-mce-maximagesize>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceMaxImageSizeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
*/
