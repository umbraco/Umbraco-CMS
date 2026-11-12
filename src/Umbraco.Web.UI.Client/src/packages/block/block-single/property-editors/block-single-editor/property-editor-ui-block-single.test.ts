import { UmbPropertyEditorUIBlockSingleElement } from './property-editor-ui-block-single.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockSingleElement', () => {
	let element: UmbPropertyEditorUIBlockSingleElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-block-single></umb-property-editor-ui-block-single> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockSingleElement);
	});

	describe('readonly', () => {
		const getButtonGroupDisabledStates = () =>
			Array.from(element.shadowRoot!.querySelectorAll('uui-button-group uui-button'), (button) =>
				button.hasAttribute('disabled'),
			);

		it('disables the create button group when readonly changes from false to true after the first render', async () => {
			expect(getButtonGroupDisabledStates()).to.not.be.empty;
			expect(getButtonGroupDisabledStates()).to.not.include(true);

			element.readonly = true;
			await element.updateComplete;

			expect(getButtonGroupDisabledStates()).to.not.include(false);
		});

		it('enables the create button group when readonly changes from true to false after the first render', async () => {
			element = await fixture(html`
				<umb-property-editor-ui-block-single .readonly=${true}></umb-property-editor-ui-block-single>
			`);
			expect(getButtonGroupDisabledStates()).to.not.be.empty;
			expect(getButtonGroupDisabledStates()).to.not.include(false);

			element.readonly = false;
			await element.updateComplete;

			expect(getButtonGroupDisabledStates()).to.not.include(true);
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
