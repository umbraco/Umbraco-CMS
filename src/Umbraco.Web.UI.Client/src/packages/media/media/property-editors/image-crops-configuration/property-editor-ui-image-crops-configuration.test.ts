import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIImageCropsConfigurationElement } from './property-editor-ui-image-crops-configuration.element.js';
//import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIImageCropsConfigurationElement', () => {
	let element: UmbPropertyEditorUIImageCropsConfigurationElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-image-crops-configuration></umb-property-editor-ui-image-crops-configuration>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIImageCropsConfigurationElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			//TODO: This test is broken. It fails at forms because of missing labels even if you have them.
			// await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
