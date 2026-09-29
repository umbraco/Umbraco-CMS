import { UmbSortableListElement } from './sortable-list.element.js';
import './sortable-list-item.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbSortableListElement', () => {
	let element: UmbSortableListElement<string>;

	const getUnique = (item: string) => item;
	const renderMethod = (item: string) =>
		html`<umb-sortable-list-item .unique=${item}>${item}</umb-sortable-list-item>`;

	beforeEach(async () => {
		element = await fixture(html`<umb-sortable-list></umb-sortable-list>`);
		element.getUnique = getUnique;
		element.renderMethod = renderMethod;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbSortableListElement);
	});

	it('renders nothing when there are no items', async () => {
		element.items = [];
		await element.updateComplete;
		const items = element.shadowRoot!.querySelectorAll('umb-sortable-list-item');
		expect(items).to.have.lengthOf(0);
	});

	it('renders an item per entry, in order', async () => {
		element.items = ['a', 'b', 'c'];
		await element.updateComplete;

		const items = element.shadowRoot!.querySelectorAll('umb-sortable-list-item');
		expect(items).to.have.lengthOf(3);
		expect(Array.from(items).map((item) => item.textContent?.trim())).to.deep.equal(['a', 'b', 'c']);
	});

	it('stamps the unique identifier as the data-sort-entry-id attribute', async () => {
		element.items = ['a', 'b', 'c'];
		await element.updateComplete;

		const items = element.shadowRoot!.querySelectorAll('umb-sortable-list-item');
		expect(Array.from(items).map((item) => item.getAttribute('data-sort-entry-id'))).to.deep.equal(['a', 'b', 'c']);
	});

	it('re-renders when items are replaced', async () => {
		element.items = ['a', 'b'];
		await element.updateComplete;
		element.items = ['b', 'c'];
		await element.updateComplete;

		const items = element.shadowRoot!.querySelectorAll('umb-sortable-list-item');
		expect(Array.from(items).map((item) => item.textContent?.trim())).to.deep.equal(['b', 'c']);
	});

	it('reflects the disabled property to an attribute', async () => {
		element.disabled = true;
		await element.updateComplete;
		expect(element.hasAttribute('disabled')).to.be.true;
	});

	it('defaults the item and handle selectors', () => {
		expect(element.itemSelector).to.equal('umb-sortable-list-item');
		expect(element.handleSelector).to.equal('.handle');
	});

	it('accepts custom item-selector and handle-selector attributes', async () => {
		const custom = await fixture<UmbSortableListElement<string>>(
			html`<umb-sortable-list item-selector=".my-item" handle-selector=".my-handle"></umb-sortable-list>`,
		);
		expect(custom.itemSelector).to.equal('.my-item');
		expect(custom.handleSelector).to.equal('.my-handle');
	});

	it('falls back to the default selectors when set to an empty string', () => {
		element.itemSelector = '';
		element.handleSelector = '';
		expect(element.itemSelector).to.equal('umb-sortable-list-item');
		expect(element.handleSelector).to.equal('.handle');
	});

	it('has no identifier by default', () => {
		expect(element.identifier).to.be.undefined;
	});

	it('accepts a custom identifier attribute', async () => {
		const custom = await fixture<UmbSortableListElement<string>>(
			html`<umb-sortable-list identifier="Umb.SorterIdentifier.Test"></umb-sortable-list>`,
		);
		expect(custom.identifier).to.equal('Umb.SorterIdentifier.Test');
	});
});
