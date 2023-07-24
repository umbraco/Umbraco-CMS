import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement } from './property-editor-ui-tiny-mce-maximagesize-configuration.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITinyMceMaxImSizeConfigurationElement', () => {
	let element: UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement;

	beforeEach(async () => {
		element = await fixture(
			html`
				<umb-property-editor-ui-tiny-mce-maximagesize-configuration></umb-property-editor-ui-tiny-mce-maximagesize-configuration>
			`
		);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceMaxImageSizeConfigurationElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
