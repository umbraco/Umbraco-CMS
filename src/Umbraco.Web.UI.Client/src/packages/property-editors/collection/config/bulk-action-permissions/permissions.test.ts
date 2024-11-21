import { UmbPropertyEditorUICollectionPermissionsElement } from './permissions.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICollectionPermissionsElement', () => {
	let element: UmbPropertyEditorUICollectionPermissionsElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-collection-permissions></umb-property-editor-ui-collection-permissions>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICollectionPermissionsElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
