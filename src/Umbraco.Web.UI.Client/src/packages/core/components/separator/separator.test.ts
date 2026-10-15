import { UmbSeparatorElement } from './separator.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbSeparatorElement', () => {
	it('is defined with its own instance', async () => {
		const element = await fixture(html`<umb-separator></umb-separator>`);
		expect(element).to.be.instanceOf(UmbSeparatorElement);
	});

	it('is exposed as a separator', async () => {
		const element = await fixture(html`<umb-separator></umb-separator>`);
		expect(element.getAttribute('role')).to.equal('separator');
	});

	it('keeps a role set by the author', async () => {
		const element = await fixture(html`<umb-separator role="presentation"></umb-separator>`);
		expect(element.getAttribute('role')).to.equal('presentation');
	});
});
