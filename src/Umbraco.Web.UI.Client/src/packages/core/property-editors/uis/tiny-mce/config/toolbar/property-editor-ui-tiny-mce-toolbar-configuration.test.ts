import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITinyMceToolbarConfigurationElement } from './property-editor-ui-tiny-mce-toolbar-configuration.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import '@umbraco-cms/backoffice/external/tinymce';

describe('UmbPropertyEditorUITinyMceToolbarConfigurationElement', () => {
	let element: UmbPropertyEditorUITinyMceToolbarConfigurationElement;

	beforeEach(async () => {
		element = await fixture(
			html`
				<umb-property-editor-ui-tiny-mce-toolbar-configuration></umb-property-editor-ui-tiny-mce-toolbar-configuration>
			`
		);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITinyMceToolbarConfigurationElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
