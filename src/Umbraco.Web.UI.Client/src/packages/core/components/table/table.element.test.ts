import { UmbTableElement, type UmbTableConfig, type UmbTableItem } from './table.element.js';
import { expect, fixture, html, oneEvent } from '@open-wc/testing';

function item(id: string, overrides: Partial<UmbTableItem> = {}): UmbTableItem {
	return { id, data: [], ...overrides };
}

function items(ids: Array<string>): Array<UmbTableItem> {
	return ids.map((id) => item(id));
}

describe('UmbTableElement', () => {
	let element: UmbTableElement;

	const config: UmbTableConfig = { allowSelection: true, allowSelectAll: true };

	beforeEach(async () => {
		element = await fixture(html`<umb-table></umb-table>`);
		element.config = config;
	});

	function getHeaderCheckbox(): HTMLInputElement {
		return element.shadowRoot!.querySelector('uui-table-head uui-checkbox') as unknown as HTMLInputElement;
	}

	async function toggleSelectAll(checked: boolean) {
		const checkbox = getHeaderCheckbox();
		checkbox.checked = checked;
		checkbox.dispatchEvent(new Event('change'));
		await element.updateComplete;
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbTableElement);
	});

	describe('select all across pages', () => {
		it('accumulates the current page into the existing selection instead of replacing it', async () => {
			// Page 1
			element.items = items(['1', '2', '3', '4']);
			await element.updateComplete;
			await toggleSelectAll(true);
			expect(element.selection).to.have.members(['1', '2', '3', '4']);

			// Navigate to page 2 — the selection persists across the page change
			element.items = items(['5', '6', '7', '8']);
			await element.updateComplete;
			await toggleSelectAll(true);

			expect(element.selection).to.have.members(['1', '2', '3', '4', '5', '6', '7', '8']);
		});

		it('does not duplicate ids already present in the selection', async () => {
			element.items = items(['1', '2', '3']);
			element.selection = ['1'];
			await element.updateComplete;
			await toggleSelectAll(true);

			expect(element.selection).to.have.members(['1', '2', '3']);
			expect(element.selection.length).to.equal(3);
		});

		it('excludes non-selectable rows from the current page', async () => {
			element.items = [item('1'), item('2', { selectable: false }), item('3')];
			await element.updateComplete;
			await toggleSelectAll(true);

			expect(element.selection).to.have.members(['1', '3']);
		});

		it('emits a selected event', async () => {
			element.items = items(['1', '2']);
			await element.updateComplete;

			const listener = oneEvent(element, 'selected');
			await toggleSelectAll(true);
			const event = await listener;

			expect(event).to.exist;
		});
	});

	describe('deselect all across pages', () => {
		it('removes only the current page, leaving other pages selected', async () => {
			element.items = items(['1', '2', '3', '4']);
			element.selection = ['1', '2', '3', '4', '5', '6', '7', '8'];
			await element.updateComplete;
			await toggleSelectAll(false);

			expect(element.selection).to.have.members(['5', '6', '7', '8']);
		});

		it('emits a deselected event', async () => {
			element.items = items(['1', '2']);
			element.selection = ['1', '2'];
			await element.updateComplete;

			const listener = oneEvent(element, 'deselected');
			await toggleSelectAll(false);
			const event = await listener;

			expect(event).to.exist;
		});
	});

	describe('header checkbox state', () => {
		it('is checked when every selectable row on the current page is selected', async () => {
			element.items = items(['1', '2', '3', '4']);
			element.selection = ['1', '2', '3', '4', '5'];
			await element.updateComplete;

			const checkbox = getHeaderCheckbox();
			expect(checkbox.checked).to.be.true;
			expect(checkbox.indeterminate).to.be.false;
		});

		it('is unchecked when no row on the current page is selected, even if other pages are', async () => {
			element.items = items(['5', '6', '7', '8']);
			element.selection = ['1', '2', '3', '4'];
			await element.updateComplete;

			const checkbox = getHeaderCheckbox();
			expect(checkbox.checked).to.be.false;
			expect(checkbox.indeterminate).to.be.false;
		});

		it('is indeterminate when only some rows on the current page are selected', async () => {
			element.items = items(['1', '2', '3', '4']);
			element.selection = ['1', '2'];
			await element.updateComplete;

			const checkbox = getHeaderCheckbox();
			expect(checkbox.checked).to.be.false;
			expect(checkbox.indeterminate).to.be.true;
		});

		it('is unchecked when the current page has no selectable rows', async () => {
			element.items = items(['1', '2']).map((i) => ({ ...i, selectable: false }));
			element.selection = [];
			await element.updateComplete;

			const checkbox = getHeaderCheckbox();
			expect(checkbox.checked).to.be.false;
			expect(checkbox.indeterminate).to.be.false;
		});
	});
});
