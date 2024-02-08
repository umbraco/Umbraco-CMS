import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICollectionViewBulkActionPermissionsElement } from './property-editor-ui-collection-view-bulk-action-permissions.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionViewBulkActionPermissionsElement', () => {
	let element: UmbPropertyEditorUICollectionViewBulkActionPermissionsElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-collection-view-bulk-action-permissions></umb-property-editor-ui-collection-view-bulk-action-permissions>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionViewBulkActionPermissionsElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
