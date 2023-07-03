import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICollectionViewLayoutConfigurationElement } from './property-editor-ui-collection-view-layout-configuration.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionViewLayoutConfigurationElement', () => {
	let element: UmbPropertyEditorUICollectionViewLayoutConfigurationElement;

	beforeEach(async () => {
		element = await fixture(
			html`
				<umb-property-editor-ui-collection-view-layout-configuration></umb-property-editor-ui-collection-view-layout-configuration>
			`
		);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionViewLayoutConfigurationElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
