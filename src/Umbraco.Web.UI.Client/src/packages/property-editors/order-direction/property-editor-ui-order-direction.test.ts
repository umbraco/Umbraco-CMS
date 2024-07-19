import { UmbPropertyEditorUIOrderDirectionElement } from './property-editor-ui-order-direction.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIOrderDirectionElement', () => {
	let element: UmbPropertyEditorUIOrderDirectionElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-order-direction></umb-property-editor-ui-order-direction> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIOrderDirectionElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
