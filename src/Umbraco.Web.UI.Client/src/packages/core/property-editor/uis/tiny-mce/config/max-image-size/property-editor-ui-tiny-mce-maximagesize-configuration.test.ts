import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement } from './property-editor-ui-tiny-mce-maximagesize-configuration.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceMaxImSizeConfigurationElement', () => {
	let element: UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-tiny-mce-maximagesize-configuration></umb-property-editor-ui-tiny-mce-maximagesize-configuration>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
