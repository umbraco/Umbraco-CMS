import { UmbPropertyEditorUIBlockListElement } from './property-editor-ui-block-list.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockListElement', () => {
	let element: UmbPropertyEditorUIBlockListElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-block-list></umb-property-editor-ui-block-list> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockListElement);
	});

	describe('readonly', () => {
		const getCreateButtonGroupButtons = () =>
			Array.from(element.shadowRoot!.querySelectorAll<HTMLElement>('uui-button-group uui-button'));

		it('disables the create button group when readonly changes from false to true after the first render', async () => {
			expect(getCreateButtonGroupButtons()).to.not.be.empty;
			expect(getCreateButtonGroupButtons().every((button) => !button.hasAttribute('disabled'))).to.be.true;

			element.readonly = true;
			await element.updateComplete;

			expect(getCreateButtonGroupButtons().every((button) => button.hasAttribute('disabled'))).to.be.true;
		});

		it('enables the create button group when readonly changes from true to false after the first render', async () => {
			element = await fixture(html`
				<umb-property-editor-ui-block-list .readonly=${true}></umb-property-editor-ui-block-list>
			`);
			expect(getCreateButtonGroupButtons()).to.not.be.empty;
			expect(getCreateButtonGroupButtons().every((button) => button.hasAttribute('disabled'))).to.be.true;

			element.readonly = false;
			await element.updateComplete;

			expect(getCreateButtonGroupButtons().every((button) => !button.hasAttribute('disabled'))).to.be.true;
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
