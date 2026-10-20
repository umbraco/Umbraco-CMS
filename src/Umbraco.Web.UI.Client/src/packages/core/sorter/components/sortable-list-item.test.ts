import { UmbSortableListItemElement } from './sortable-list-item.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbSortableListItemElement', () => {
	let element: UmbSortableListItemElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-sortable-list-item></umb-sortable-list-item>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbSortableListItemElement);
	});

	it('renders the drag handle and actions by default', () => {
		expect(element.shadowRoot!.querySelector('.handle')).to.exist;
		expect(element.shadowRoot!.querySelector('.actions')).to.exist;
	});

	it('hides the drag handle when hide-handle is set', async () => {
		element.hideHandle = true;
		await element.updateComplete;
		expect(element.shadowRoot!.querySelector('.handle')).to.not.exist;
		expect(element.shadowRoot!.querySelector('.actions')).to.exist;
	});

	it('hides the actions when hide-actions is set', async () => {
		element.hideActions = true;
		await element.updateComplete;
		expect(element.shadowRoot!.querySelector('.actions')).to.not.exist;
		expect(element.shadowRoot!.querySelector('.handle')).to.exist;
	});

	it('hides both the drag handle and actions when disabled', async () => {
		element.disabled = true;
		await element.updateComplete;
		expect(element.shadowRoot!.querySelector('.handle')).to.not.exist;
		expect(element.shadowRoot!.querySelector('.actions')).to.not.exist;
	});

	it('reflects the unique property to the data-sort-entry-id attribute', async () => {
		element.unique = 'my-unique-id';
		await element.updateComplete;
		expect(element.getAttribute('data-sort-entry-id')).to.equal('my-unique-id');
	});
});
