import { UmbBodyLayoutElement } from './body-layout.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbBodyLayoutElement', () => {
	it('scrolls the main content area to the top', async () => {
		const element = await fixture<UmbBodyLayoutElement>(
			html`<umb-body-layout style="height: 100px"><div style="height: 1000px"></div></umb-body-layout>`,
		);

		// Scroll view down:
		const scrollContainer = element.shadowRoot!.querySelector('#main') as HTMLElement;
		scrollContainer.scrollTop = 400;
		expect(scrollContainer.scrollTop).to.be.greaterThan(0);

		// Ask umb-body-layout to scroll to the top:
		element.getScrollContainer()?.scrollTo({ top: 0, behavior: 'instant' });

		expect(scrollContainer.scrollTop).to.equal(0);
	});
});
