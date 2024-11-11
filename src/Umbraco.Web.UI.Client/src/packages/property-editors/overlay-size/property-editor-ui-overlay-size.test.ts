import { UmbPropertyEditorUIOverlaySizeElement } from './property-editor-ui-overlay-size.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIOverlaySizeElement', () => {
	let element: UmbPropertyEditorUIOverlaySizeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-overlay-size></umb-property-editor-ui-overlay-size> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIOverlaySizeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
