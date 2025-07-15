import { UmbPropertyEditorUIValueTypeElement } from './property-editor-ui-value-type.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIValueTypeElement', () => {
	let element: UmbPropertyEditorUIValueTypeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-value-type></umb-property-editor-ui-value-type> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIValueTypeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
