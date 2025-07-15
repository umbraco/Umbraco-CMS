import { UmbPropertyEditorUIMultipleTextStringElement } from './property-editor-ui-multiple-text-string.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMultipleTextStringElement', () => {
	let element: UmbPropertyEditorUIMultipleTextStringElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-multiple-text-string></umb-property-editor-ui-multiple-text-string>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIMultipleTextStringElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
