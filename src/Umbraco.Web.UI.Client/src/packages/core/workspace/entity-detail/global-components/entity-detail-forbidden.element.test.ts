import { UmbEntityDetailForbiddenElement } from './entity-detail-forbidden.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbEntityDetailForbiddenElement', () => {
	let element: UmbEntityDetailForbiddenElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-entity-detail-forbidden></umb-entity-detail-forbidden>`);
	});

	function text() {
		return element.shadowRoot?.textContent ?? '';
	}

	function headline() {
		return element.shadowRoot?.querySelector('h4')?.textContent;
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEntityDetailForbiddenElement);
	});

	it('shows the default access denied text', () => {
		expect(headline()).to.not.be.empty;
		expect(text()).to.not.contain('Locked');
	});

	it('shows the reason instead of the default text', async () => {
		const defaultText = text();
		element.reason = { title: 'Locked', detail: 'Unlock it first' };
		await element.updateComplete;

		expect(headline()).to.equal('Locked');
		expect(text()).to.contain('Unlock it first');
		expect(text()).to.not.equal(defaultText);
	});

	it('shows only the title of a reason without detail', async () => {
		element.reason = { title: 'Locked' };
		await element.updateComplete;

		expect(headline()).to.equal('Locked');
		expect(text().trim()).to.equal('Locked');
	});

	it('renders the detail as text, not HTML', async () => {
		element.reason = { title: 'Locked', detail: '<b>Unlock</b> it first' };
		await element.updateComplete;

		expect(element.shadowRoot?.querySelector('b')).to.be.null;
		expect(text()).to.contain('<b>Unlock</b> it first');
	});
});
