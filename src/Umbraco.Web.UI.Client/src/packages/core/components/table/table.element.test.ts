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
			expect(element.selection).to.have.lengthOf(3);
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

	describe('select only', () => {
		function getRow(id: string): HTMLElement {
			return element.shadowRoot!.querySelector(`uui-table-row[data-sortable-id="${id}"]`) as HTMLElement;
		}

		it('turns every row into select-only when a selection exists', async () => {
			element.items = items(['1', '2']);
			element.selection = ['1'];
			await element.updateComplete;

			expect(getRow('1').hasAttribute('select-only')).to.be.true;
			expect(getRow('2').hasAttribute('select-only')).to.be.true;
		});

		it('keeps a row interactive when it opts out, even while a selection exists', async () => {
			element.items = [item('1'), item('2', { selectOnly: false })];
			element.selection = ['1'];
			await element.updateComplete;

			expect(getRow('2').hasAttribute('select-only')).to.be.false;
			// The row is still part of selection mode, so it presents its checkbox.
			expect(getRow('2').hasAttribute('data-selection-mode')).to.be.true;
		});

		it('keeps a row interactive when it opts out of a select-only configuration', async () => {
			element.config = { ...config, selectOnly: true };
			element.items = [item('1'), item('2', { selectOnly: false })];
			await element.updateComplete;

			expect(getRow('1').hasAttribute('select-only')).to.be.true;
			expect(getRow('2').hasAttribute('select-only')).to.be.false;
		});
	});

	describe('children indicator', () => {
		const customSymbol = () => html`<span class="custom-symbol"></span>`;

		function getIndicatorCell(id: string): HTMLElement {
			return element.shadowRoot!.querySelector(
				`uui-table-row[data-sortable-id="${id}"] .children-indicator-cell`,
			) as HTMLElement;
		}

		it('renders the default expand symbol when no symbol is provided', async () => {
			element.items = [item('1', { childrenIndicator: { href: '/open' } })];
			await element.updateComplete;

			const cell = getIndicatorCell('1');
			expect(cell.querySelector('uui-button[data-mark="table-row:open"] uui-symbol-expand')).to.exist;
		});

		it('renders a provided symbol inside the link when the indicator has an href', async () => {
			element.items = [item('1', { childrenIndicator: { href: '/open', renderExpandSymbol: customSymbol } })];
			await element.updateComplete;

			const link = getIndicatorCell('1').querySelector('uui-button[data-mark="table-row:open"]')!;
			expect(link.getAttribute('href')).to.equal('/open');
			expect(link.querySelector('.custom-symbol')).to.exist;
			expect(link.querySelector('uui-symbol-expand')).to.not.exist;
		});

		it('renders a provided symbol inside the button when the indicator has an open handler', async () => {
			element.items = [item('1', { childrenIndicator: { onOpen: () => {}, renderExpandSymbol: customSymbol } })];
			await element.updateComplete;

			const button = getIndicatorCell('1').querySelector('uui-button[data-mark="table-row:open"]')!;
			expect(button.hasAttribute('href')).to.be.false;
			expect(button.querySelector('.custom-symbol')).to.exist;
			expect(button.querySelector('uui-symbol-expand')).to.not.exist;
		});

		it('renders a provided symbol on its own when the indicator has nowhere to lead', async () => {
			element.items = [item('1', { childrenIndicator: { renderExpandSymbol: customSymbol } })];
			await element.updateComplete;

			const cell = getIndicatorCell('1');
			expect(cell.querySelector('.custom-symbol')).to.exist;
			expect(cell.querySelector('uui-button')).to.not.exist;
			expect(cell.querySelector('uui-symbol-expand')).to.not.exist;
		});

		it('keeps the default expand symbol on rows that do not provide one', async () => {
			element.items = [
				item('1', { childrenIndicator: { href: '/open', renderExpandSymbol: customSymbol } }),
				item('2', { childrenIndicator: { href: '/open' } }),
			];
			await element.updateComplete;

			expect(getIndicatorCell('1').querySelector('.custom-symbol')).to.exist;
			expect(getIndicatorCell('2').querySelector('.custom-symbol')).to.not.exist;
			expect(getIndicatorCell('2').querySelector('uui-symbol-expand')).to.exist;
		});

		it('invokes the open handler when a provided symbol is activated', async () => {
			let opened = 0;
			element.items = [item('1', { childrenIndicator: { onOpen: () => opened++, renderExpandSymbol: customSymbol } })];
			await element.updateComplete;

			getIndicatorCell('1')
				.querySelector('uui-button')!
				.dispatchEvent(new MouseEvent('click', { bubbles: true }));

			expect(opened).to.equal(1);
		});

		it('leaves rows without an indicator without an indicator', async () => {
			element.items = [item('1', { childrenIndicator: { href: '/open' } }), item('2')];
			await element.updateComplete;

			expect(getIndicatorCell('2').children.length).to.equal(0);
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
