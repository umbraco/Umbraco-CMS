import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICollectionViewColumnConfigurationElement } from './property-editor-ui-collection-view-column-configuration.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionViewColumnConfigurationElement', () => {
	let element: UmbPropertyEditorUICollectionViewColumnConfigurationElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-collection-view-column-configuration></umb-property-editor-ui-collection-view-column-configuration>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionViewColumnConfigurationElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
