import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockGridTypeConfigurationElement } from './property-editor-ui-block-grid-type-configuration.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockGridBlockConfigurationElement', () => {
	let element: UmbPropertyEditorUIBlockGridTypeConfigurationElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-block-grid-type-configuration></umb-property-editor-ui-block-grid-type-configuration>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockGridTypeConfigurationElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
