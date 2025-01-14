import { UmbPropertyEditorUIUserPickerElement } from './property-editor-ui-user-picker.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIUserPickerElement', () => {
	let element: UmbPropertyEditorUIUserPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-user-picker></umb-property-editor-ui-user-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIUserPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
