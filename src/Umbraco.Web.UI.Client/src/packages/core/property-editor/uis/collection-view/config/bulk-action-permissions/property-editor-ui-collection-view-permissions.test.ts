import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICollectionViewPermissionsElement } from './property-editor-ui-collection-view-permissions.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionViewPermissionsElement', () => {
	let element: UmbPropertyEditorUICollectionViewPermissionsElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-collection-view-permissions></umb-property-editor-ui-collection-view-permissions>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionViewPermissionsElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
