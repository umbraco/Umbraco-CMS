import { UmbSorterController } from './sorter.controller.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '../lit-element/lit-element.element.js';

@customElement('test-my-sorter')
class UmbSorterTestElement extends UmbLitElement {
	model: Array<string> = ['1', '2', '3'];

	sorter = new UmbSorterController<string, HTMLElement>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.Test',
		itemSelector: 'li',
		containerSelector: 'ul',
		onChange: ({ model }) => {
			this.model = model;
		},
	});

	getAllItems() {
		return Array.from(this.shadowRoot!.querySelectorAll('li'));
	}

	override render() {
		return html`<ul>
			<li id="1">Item 1</li>
			<li id="2">Item 2</li>
			<li id="3">Item 3</li>
		</ul>`;
	}
}

describe('UmbSorterController', () => {
	let element: UmbSorterTestElement;

	beforeEach(async () => {
		element = await fixture(html`<test-my-sorter></test-my-sorter>`);
		await aTimeout(10);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbSorterTestElement);
	});

	describe('Set up', () => {
		it('should find all items', () => {
			const items = element.getAllItems();
			expect(items.length).to.equal(3);
		});

		it('sets all allowed draggable items to draggable', () => {
			const items = element.getAllItems();
			items.forEach((item) => {
				expect(item.draggable).to.be.true;
			});
		});
	});

	describe('disable', () => {
		it('sets all items to non draggable', () => {
			element.sorter.disable();
			const items = element.getAllItems();
			items.forEach((item) => {
				expect(item.draggable).to.be.false;
			});
		});
	});

	describe('enable', () => {
		it('sets all items to draggable', () => {
			const items = element.getAllItems();
			items.forEach((item) => {
				expect(item.draggable).to.be.true;
			});
		});
	});
});
