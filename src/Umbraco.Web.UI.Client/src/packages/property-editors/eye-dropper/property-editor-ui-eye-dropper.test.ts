import { UmbPropertyEditorUIEyeDropperElement } from './property-editor-ui-eye-dropper.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIEyeDropperElement', () => {
	let element: UmbPropertyEditorUIEyeDropperElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-eye-dropper></umb-property-editor-ui-eye-dropper> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIEyeDropperElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
