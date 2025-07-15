import { UmbPropertyEditorUIUploadFieldElement } from './property-editor-ui-upload-field.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIUploadFieldElement', () => {
	let element: UmbPropertyEditorUIUploadFieldElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-upload-field></umb-property-editor-ui-upload-field> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIUploadFieldElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
