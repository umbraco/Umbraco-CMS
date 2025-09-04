import { UmbPropertyEditorUiTiptapElement } from './property-editor-ui-tiptap.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITiptapElement', () => {
	let element: UmbPropertyEditorUiTiptapElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-tiptap></umb-property-editor-ui-tiptap> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUiTiptapElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
