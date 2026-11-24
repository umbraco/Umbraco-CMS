import { UmbPropertyEditorUIBlockGridElement } from './property-editor-ui-block-grid.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockGridElement', () => {
	let element: UmbPropertyEditorUIBlockGridElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-block-grid></umb-property-editor-ui-block-grid> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockGridElement);
	});

	describe('readonly', () => {
		it('reflects readonly changes after the first render to the attribute', async () => {
			expect(element.hasAttribute('readonly')).to.be.false;

			element.readonly = true;
			await element.updateComplete;

			expect(element.hasAttribute('readonly')).to.be.true;

			element.readonly = false;
			await element.updateComplete;

			expect(element.hasAttribute('readonly')).to.be.false;
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
