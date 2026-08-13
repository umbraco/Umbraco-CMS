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

	function collectionIndicator(): Element | null {
		return element.shadowRoot!.querySelector('#is-collection');
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

	it('renders no collection indicator by default', () => {
		expect(collectionIndicator()).to.be.null;
	});

	it('renders a collection indicator when it is a collection', async () => {
		element.isCollection = true;
		await element.updateComplete;

		expect(collectionIndicator()).to.exist;
	});

	it('renders the collection indicator instead of the children indicator when it is a collection with children', async () => {
		element.hasChildren = true;
		element.isCollection = true;
		await element.updateComplete;

		expect(collectionIndicator()).to.exist;
		expect(childrenIndicator()).to.be.null;
	});

	it('hides the collection indicator from assistive technology', async () => {
		element.isCollection = true;
		await element.updateComplete;

		expect(collectionIndicator()!.getAttribute('aria-hidden')).to.equal('true');
	});
});
