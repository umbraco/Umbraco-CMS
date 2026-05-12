import { UmbEntityFrameElement } from './entity-frame.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbEntityFrameElement', () => {
	let element: UmbEntityFrameElement;

	beforeEach(async () => {
		element = await fixture<UmbEntityFrameElement>(html`<umb-entity-frame></umb-entity-frame>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEntityFrameElement);
	});

	it('renders a border element', () => {
		const border = element.shadowRoot!.querySelector('.border');
		expect(border).to.not.equal(null);
	});

	it('renders a tab with a default slot', () => {
		const slot = element.shadowRoot!.querySelector('.tab slot');
		expect(slot).to.not.equal(null);
	});

	it('has a label property that defaults to an empty string', () => {
		expect(element.label).to.equal('');
	});

	it('renders the label inside the tab when no slot content is projected', async () => {
		element.label = 'Document: Hero';
		await element.updateComplete;
		const tab = element.shadowRoot!.querySelector('.tab')!;
		expect(tab.textContent?.trim()).to.equal('Document: Hero');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
