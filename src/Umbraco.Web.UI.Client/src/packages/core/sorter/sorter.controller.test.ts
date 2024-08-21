import { UmbSorterController } from './sorter.controller.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '../lit-element/lit-element.element.js';

@customElement('test-my-sorter')
class UmbSorterTestElement extends UmbLitElement {
	model: Array<string> = ['1', '2', '3', '4'];

	sorter = new UmbSorterController<string, HTMLElement>(this, {
		getUniqueOfElement: (element) => {
			return element.id;
		},
		getUniqueOfModel: (modelEntry) => {
			return modelEntry;
		},
		identifier: 'Umb.SorterIdentifier.Test',
		itemSelector: '.item',
		containerSelector: '#container',
		disabledItemSelector: '.disabled',
		onChange: ({ model }) => {
			this.model = model;
		},
	});

	getAllItems() {
		return Array.from(this.shadowRoot!.querySelectorAll('.item')) as HTMLElement[];
	}

	getSortableItems() {
		return Array.from(this.shadowRoot!.querySelectorAll('.item:not(.disabled')) as HTMLElement[];
	}

	getDisabledItems() {
		return Array.from(this.shadowRoot!.querySelectorAll('.item.disabled')) as HTMLElement[];
	}

	override render() {
		return html`<div id="container">
			<div id="1" class="item">Item 1</div>
			<div id="2" class="item">Item 2</div>
			<div id="3" class="item disabled">Item 3</div>
			<div id="4" class="item">Item 4</div>
		</div>`;
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
			expect(items.length).to.equal(4);
		});

		it('sets all allowed draggable items to draggable', () => {
			const items = element.getSortableItems();
			expect(items.length).to.equal(3);
			items.forEach((item) => {
				expect(item.draggable).to.be.true;
			});
		});

		it('sets all disabled items non draggable', () => {
			const items = element.getDisabledItems();
			expect(items.length).to.equal(1);
			items.forEach((item) => {
				expect(item.draggable).to.be.false;
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
		it('sets all allowed items to draggable', () => {
			const items = element.getSortableItems();
			expect(items.length).to.equal(3);
			items.forEach((item) => {
				expect(item.draggable).to.be.true;
			});
		});

		it('sets all disabled items non draggable', () => {
			const items = element.getDisabledItems();
			expect(items.length).to.equal(1);
			items.forEach((item) => {
				expect(item.draggable).to.be.false;
			});
		});
	});
});
