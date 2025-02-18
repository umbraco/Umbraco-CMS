/*
// Disabled because TinyMCE is not included in the test runner
import { UmbPropertyEditorUITinyMceDimensionsConfigurationElement } from './property-editor-ui-tiny-mce-dimensions-configuration.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceDimensionsConfigurationElement', () => {
	let element: UmbPropertyEditorUITinyMceDimensionsConfigurationElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-tiny-mce-dimensions-configuration></umb-property-editor-ui-tiny-mce-dimensions-configuration>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceDimensionsConfigurationElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
*/
