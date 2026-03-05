/* eslint-disable local-rules/enforce-umbraco-external-imports */
import type { Editor, ProseMirrorNode } from '../../../externals.js';
import type { UmbTiptapMenuElement } from '../../../components/menu/tiptap-menu.element.js';
import { CellSelection, TableMap } from '@tiptap/pm/tables';
import { Plugin } from '@tiptap/pm/state';
import type { EditorView } from '@tiptap/pm/view';
import type { PluginView, Selection, Transaction } from '@tiptap/pm/state';
import type { Rect } from '@tiptap/pm/tables';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

let nextPopoverId = 0;

interface TableHandlesState {
	rowIndex: number | undefined;
	colIndex: number | undefined;
	referencePosCell: DOMRect | undefined;
	referencePosTable: DOMRect | undefined;
	tableNode: ProseMirrorNode | undefined;
	tablePos: number | undefined;
	controlsContainer: HTMLElement | undefined;
}

/**
 * Plugin view that manages table grips and shared popover menu.
 * Consolidates grip management from header/cell plugins into a single location.
 */
class TableHandlePluginView implements PluginView {
	#editor: Editor;
	#view: EditorView;
	#state: TableHandlesState | undefined;

	#rowGrip: HTMLElement | null = null;
	#colGrip: HTMLElement | null = null;
	#rowPopover: UUIPopoverContainerElement | null = null;
	#colPopover: UUIPopoverContainerElement | null = null;
	#rowMenu: UmbTiptapMenuElement | null = null;
	#colMenu: UmbTiptapMenuElement | null = null;
	#currentControlsContainer: HTMLElement | null = null;

	constructor(editor: Editor, view: EditorView) {
		this.#editor = editor;
		this.#view = view;

		// Setup event listeners
		this.#view.dom.addEventListener('mousemove', this.#handleMouseMove);
		this.#view.dom.addEventListener('mouseleave', this.#handleMouseLeave);
	}

	#handleMouseMove = (event: MouseEvent) => {
		if (!this.#editor.isEditable) {
			this.#hideGrips();
			return;
		}

		const target = event.target as HTMLElement;
		if (!target) return;

		// Don't update state if hovering over grips or popover - keep them visible
		if (this.#isOverGripOrPopover(target)) {
			return;
		}

		// Only process if inside the editor
		if (!this.#view.dom.contains(target)) return;

		this.#updateStateFromMousePosition(event);
	};

	#handleMouseLeave = () => {
		this.#hideGrips();
	};

	#isOverGripOrPopover(target: HTMLElement): boolean {
		// Check if target is a grip or inside a grip
		if (target.closest('.grip-row') || target.closest('.grip-column')) {
			return true;
		}
		// Check if target is inside the table-controls container (grip area)
		if (target.closest('.table-controls')) {
			return true;
		}
		// Check if target is a popover or inside a popover
		if (target.closest('uui-popover-container')) {
			return true;
		}
		return false;
	}

	#updateStateFromMousePosition(event: MouseEvent) {
		// Find the cell element under the mouse
		const cellElement = this.#findCellFromEvent(event);

		if (!cellElement) {
			this.#hideGrips();
			return;
		}

		// Find the table and its position
		const tableInfo = this.#findTableFromCell(cellElement);
		if (!tableInfo) {
			this.#hideGrips();
			return;
		}

		// Get the controls container from the UmbTableView
		const tableWrapper = cellElement.closest('.tableWrapper');
		const controlsContainer = tableWrapper?.querySelector('.table-controls') as HTMLElement | undefined;

		if (!controlsContainer) {
			this.#hideGrips();
			return;
		}

		// Get cell indices using TableMap for correct handling of merged cells
		const cellIndices = this.#getCellIndices(cellElement, tableInfo);
		if (!cellIndices) {
			this.#hideGrips();
			return;
		}

		const tableBody = cellElement.closest('tbody') || cellElement.closest('table');
		const tableRect = tableBody?.getBoundingClientRect();
		const cellRect = cellElement.getBoundingClientRect();

		this.#state = {
			rowIndex: cellIndices.rowIndex,
			colIndex: cellIndices.colIndex,
			referencePosCell: cellRect,
			referencePosTable: tableRect,
			tableNode: tableInfo.node,
			tablePos: tableInfo.pos,
			controlsContainer: controlsContainer,
		};

		this.#updateGrips(controlsContainer);
	}

	#findCellFromEvent(event: MouseEvent): HTMLTableCellElement | null {
		const target = event.target as HTMLElement;
		const cell = target.closest('td, th') as HTMLTableCellElement | null;
		return cell;
	}

	#findTableFromCell(cell: HTMLElement): { node: ProseMirrorNode; pos: number } | null {
		const tableElement = cell.closest('table');
		if (!tableElement) return null;

		// Find the table wrapper which is the nodeDOM for the table
		const tableWrapper = tableElement.closest('.tableWrapper');
		if (!tableWrapper) return null;

		// Get the block container (our enhanced UmbTableView's dom)
		const blockContainer = tableWrapper.parentElement;
		if (!blockContainer?.classList.contains('umb-tiptap-table')) return null;

		// Find the position in the document
		const pos = this.#view.posAtDOM(tableElement, 0);
		if (pos === undefined) return null;

		try {
			const $pos = this.#view.state.doc.resolve(pos);
			for (let d = $pos.depth; d >= 0; d--) {
				const node = $pos.node(d);
				if (node.type.name === 'table') {
					return { node, pos: d === 0 ? 0 : $pos.before(d) };
				}
			}
		} catch {
			return null;
		}

		return null;
	}

	#getCellIndices(
		cell: HTMLTableCellElement,
		tableInfo: { node: ProseMirrorNode; pos: number },
	): { rowIndex: number; colIndex: number } | null {
		try {
			const pos = this.#view.posAtDOM(cell, 0);
			const $pos = this.#view.state.doc.resolve(pos);
			const tableStart = tableInfo.pos + 1;
			const map = TableMap.get(tableInfo.node);

			// Walk up from the resolved position to find the cell node
			for (let d = $pos.depth; d >= 0; d--) {
				const name = $pos.node(d).type.name;
				if (name === 'tableCell' || name === 'tableHeader') {
					const rect = map.findCell($pos.before(d) - tableStart);
					return { rowIndex: rect.top, colIndex: rect.left };
				}
			}
		} catch {
			// Position resolution or cell lookup failed
		}

		return null;
	}

	#hideGrips() {
		if (this.#rowGrip) {
			this.#rowGrip.style.display = 'none';
		}
		if (this.#colGrip) {
			this.#colGrip.style.display = 'none';
		}
		this.#state = undefined;
	}

	#updateGrips(container: HTMLElement) {
		// Create grips and popover if they don't exist or if container changed
		if (this.#currentControlsContainer !== container) {
			this.#cleanupGrips();
			this.#createGrips(container);
			this.#currentControlsContainer = container;
		}

		this.#positionGrips();
	}

	#createGrips(container: HTMLElement) {
		// Generate unique IDs for each popover
		const id = nextPopoverId++;
		const rowPopoverId = `table-row-menu-${id}`;
		const colPopoverId = `table-col-menu-${id}`;

		// Create row grip as button
		const rowGrip = document.createElement('button');
		rowGrip.type = 'button';
		rowGrip.className = 'grip-row';
		rowGrip.appendChild(document.createElement('uui-symbol-more'));
		rowGrip.setAttribute('popovertarget', rowPopoverId);
		rowGrip.addEventListener('click', () => {
			this.#handleGripClick('row');
		});
		this.#rowGrip = rowGrip;

		// Create column grip as button
		const colGrip = document.createElement('button');
		colGrip.type = 'button';
		colGrip.className = 'grip-column';
		colGrip.appendChild(document.createElement('uui-symbol-more'));
		colGrip.setAttribute('popovertarget', colPopoverId);
		colGrip.addEventListener('click', () => {
			this.#handleGripClick('column');
		});
		this.#colGrip = colGrip;

		// Create row popover
		this.#rowPopover = document.createElement('uui-popover-container') as UUIPopoverContainerElement;
		this.#rowPopover.id = rowPopoverId;
		this.#rowPopover.setAttribute('popover', 'auto');
		this.#rowPopover.placement = 'left';

		// Create row menu element
		this.#rowMenu = document.createElement('umb-tiptap-menu') as UmbTiptapMenuElement;
		this.#rowMenu.menuAlias = 'Umb.Menu.Tiptap.TableRow';
		this.#rowPopover.appendChild(this.#rowMenu);

		// Create column popover
		this.#colPopover = document.createElement('uui-popover-container') as UUIPopoverContainerElement;
		this.#colPopover.id = colPopoverId;
		this.#colPopover.setAttribute('popover', 'auto');
		this.#colPopover.placement = 'top';

		// Create column menu element
		this.#colMenu = document.createElement('umb-tiptap-menu') as UmbTiptapMenuElement;
		this.#colMenu.menuAlias = 'Umb.Menu.Tiptap.TableColumn';
		this.#colPopover.appendChild(this.#colMenu);

		container.append(this.#rowGrip, this.#colGrip, this.#rowPopover, this.#colPopover);
	}

	#handleGripClick(context: 'row' | 'column') {
		if (!this.#state || !this.#editor.isEditable) return;

		const index = context === 'row' ? this.#state.rowIndex : this.#state.colIndex;
		if (index === undefined) return;

		// Select the row or column
		const tr = this.#editor.state.tr;
		const selectFn = context === 'row' ? selectRow : selectColumn;
		const tableStart = this.#state.tablePos! + 1; // start = pos + 1 (position of first child)
		this.#editor.view.dispatch(selectFn(index)(tr, this.#state.tableNode!, tableStart));

		// Update grip selection state
		this.#updateGripSelectionState(context);
	}

	#updateGripSelectionState(selectedContext: 'row' | 'column') {
		if (this.#rowGrip) {
			this.#rowGrip.classList.toggle('selected', selectedContext === 'row');
		}
		if (this.#colGrip) {
			this.#colGrip.classList.toggle('selected', selectedContext === 'column');
		}
	}

	#positionGrips() {
		if (!this.#state?.referencePosCell || !this.#state?.referencePosTable || !this.#state?.controlsContainer) return;

		const { referencePosCell, referencePosTable, controlsContainer } = this.#state;

		// Compute the offset dynamically from the controls container's position
		const controlsRect = controlsContainer.getBoundingClientRect();
		const containerOffsetTop = referencePosTable.top - controlsRect.top;
		const containerOffsetLeft = referencePosTable.left - controlsRect.left;

		// Position row grip (left side of cell)
		if (this.#rowGrip) {
			this.#rowGrip.style.display = 'flex';
			this.#rowGrip.style.top = `${referencePosCell.top - referencePosTable.top + containerOffsetTop}px`;
			this.#rowGrip.style.height = `${referencePosCell.height}px`;
		}

		// Position column grip (top of cell)
		if (this.#colGrip) {
			this.#colGrip.style.display = 'flex';
			this.#colGrip.style.left = `${referencePosCell.left - referencePosTable.left + containerOffsetLeft}px`;
			this.#colGrip.style.width = `${referencePosCell.width}px`;
		}
	}

	#cleanupGrips() {
		this.#rowGrip?.remove();
		this.#colGrip?.remove();
		this.#rowPopover?.remove();
		this.#colPopover?.remove();
		this.#rowGrip = null;
		this.#colGrip = null;
		this.#rowPopover = null;
		this.#colPopover = null;
		this.#rowMenu = null;
		this.#colMenu = null;
	}

	update(view: EditorView) {
		this.#view = view;

		// Update grip selection state based on current selection
		const { selection } = view.state;
		if (isCellSelection(selection) && this.#state) {
			const rowSelected = this.#state.rowIndex !== undefined && isRowSelected(this.#state.rowIndex)(selection);
			const colSelected = this.#state.colIndex !== undefined && isColumnSelected(this.#state.colIndex)(selection);

			if (this.#rowGrip) {
				this.#rowGrip.classList.toggle('selected', rowSelected);
			}
			if (this.#colGrip) {
				this.#colGrip.classList.toggle('selected', colSelected);
			}
		}
	}

	destroy() {
		this.#view.dom.removeEventListener('mousemove', this.#handleMouseMove);
		this.#view.dom.removeEventListener('mouseleave', this.#handleMouseLeave);
		this.#cleanupGrips();
	}
}

/**
 * Creates the TableHandlePlugin for managing table grips and context menus.
 * @param {Editor} editor The Tiptap editor instance to which the plugin will be attached.
 * @returns {Plugin} Returns a ProseMirror plugin instance that manages table grips and context menus.
 */
export const TableHandlePlugin = (editor: Editor): Plugin => {
	return new Plugin({
		view(editorView) {
			return new TableHandlePluginView(editor, editorView);
		},
	});
};

const select =
	(type: 'row' | 'column') => (index: number) => (tr: Transaction, tableNode: ProseMirrorNode, tableStart: number) => {
		const isRowSelection = type === 'row';
		const map = TableMap.get(tableNode);

		// Check if the index is valid
		if (index >= 0 && index < (isRowSelection ? map.height : map.width)) {
			const left = isRowSelection ? 0 : index;
			const top = isRowSelection ? index : 0;
			const right = isRowSelection ? map.width : index + 1;
			const bottom = isRowSelection ? index + 1 : map.height;

			const cellsInFirstRow = map.cellsInRect({
				left,
				top,
				right: isRowSelection ? right : left + 1,
				bottom: isRowSelection ? top + 1 : bottom,
			});

			const cellsInLastRow =
				bottom - top === 1
					? cellsInFirstRow
					: map.cellsInRect({
							left: isRowSelection ? left : right - 1,
							top: isRowSelection ? bottom - 1 : top,
							right,
							bottom,
						});

			const head = tableStart + cellsInFirstRow[0];
			const anchor = tableStart + cellsInLastRow[cellsInLastRow.length - 1];
			const $head = tr.doc.resolve(head);
			const $anchor = tr.doc.resolve(anchor);

			return tr.setSelection(new CellSelection($anchor, $head));
		}

		return tr;
	};

const isRectSelected = (rect: Rect) => (selection: CellSelection) => {
	const map = TableMap.get(selection.$anchorCell.node(-1));
	const start = selection.$anchorCell.start(-1);
	const cells = map.cellsInRect(rect);
	const selectedCells = map.cellsInRect(
		map.rectBetween(selection.$anchorCell.pos - start, selection.$headCell.pos - start),
	);

	for (let i = 0, count = cells.length; i < count; i += 1) {
		if (selectedCells.indexOf(cells[i]) === -1) {
			return false;
		}
	}

	return true;
};

const isCellSelection = (selection: Selection): selection is CellSelection => selection instanceof CellSelection;

const isColumnSelected = (columnIndex: number) => (selection: Selection) => {
	if (isCellSelection(selection)) {
		const map = TableMap.get(selection.$anchorCell.node(-1));

		return isRectSelected({
			left: columnIndex,
			right: columnIndex + 1,
			top: 0,
			bottom: map.height,
		})(selection);
	}

	return false;
};

const isRowSelected = (rowIndex: number) => (selection: Selection) => {
	if (isCellSelection(selection)) {
		const map = TableMap.get(selection.$anchorCell.node(-1));

		return isRectSelected({
			left: 0,
			right: map.width,
			top: rowIndex,
			bottom: rowIndex + 1,
		})(selection);
	}

	return false;
};

const selectColumn = select('column');

const selectRow = select('row');
