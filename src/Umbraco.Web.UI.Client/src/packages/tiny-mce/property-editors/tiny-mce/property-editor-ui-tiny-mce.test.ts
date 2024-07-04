import { UmbPropertyEditorUITinyMceElement } from './property-editor-ui-tiny-mce.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceElement', () => {
	let element: UmbPropertyEditorUITinyMceElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-tiny-mce></umb-property-editor-ui-tiny-mce> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
