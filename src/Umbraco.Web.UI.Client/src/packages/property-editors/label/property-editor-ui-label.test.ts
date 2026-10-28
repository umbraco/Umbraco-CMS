import { UmbPropertyEditorUILabelElement } from './property-editor-ui-label.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUILabelElement', () => {
	let element: UmbPropertyEditorUILabelElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-label></umb-property-editor-ui-label> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUILabelElement);
	});

	it('keeps a long unbroken value within the available width', async () => {
		const container = await fixture<HTMLDivElement>(html`
			<div style="width: 200px;">
				<umb-property-editor-ui-label
					.value=${'9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08'}></umb-property-editor-ui-label>
			</div>
		`);

		expect(container.scrollWidth).to.be.at.most(container.clientWidth);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
