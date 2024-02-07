/*import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIMarkdownEditorElement } from './property-editor-ui-markdown-editor.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMarkdownEditorElement', () => {
	let element: UmbPropertyEditorUIMarkdownEditorElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-markdown-editor></umb-property-editor-ui-markdown-editor> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIMarkdownEditorElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
*/
