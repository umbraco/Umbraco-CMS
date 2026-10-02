import { UmbFigureCardElement } from './figure-card.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbFigureCardElement', () => {
	let element: UmbFigureCardElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-figure-card name="Card Name"></umb-figure-card>`);
	});

	function childrenIndicator(): Element | null {
		return element.shadowRoot!.querySelector('#has-children');
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbFigureCardElement);
	});

	it('renders no children indicator by default', () => {
		expect(childrenIndicator()).to.be.null;
	});

	it('renders a children indicator when it has children', async () => {
		element.hasChildren = true;
		await element.updateComplete;

		expect(childrenIndicator()).to.exist;
	});

	it('renders a children indicator when the has-children attribute is set', async () => {
		element.setAttribute('has-children', '');
		await element.updateComplete;

		expect(childrenIndicator()).to.exist;
	});

	it('hides the children indicator from assistive technology', async () => {
		element.hasChildren = true;
		await element.updateComplete;

		expect(childrenIndicator()!.getAttribute('aria-hidden')).to.equal('true');
	});
});
