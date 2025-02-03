import { UmbPropertyEditorUIAcceptedUploadTypesElement } from './property-editor-ui-accepted-upload-types.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIUploadFieldElement', () => {
	let element: UmbPropertyEditorUIAcceptedUploadTypesElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-accepted-upload-types></umb-property-editor-ui-accepted-upload-types>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIAcceptedUploadTypesElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
