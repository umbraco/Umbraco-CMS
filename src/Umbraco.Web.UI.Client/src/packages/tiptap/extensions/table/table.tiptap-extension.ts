/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { findParentNode, Table, TableCell, TableHeader, TableRow } from '../../externals.js';
import type { Editor } from '../../externals.js';
import type { UmbTiptapMenuElement } from '../../components/menu/tiptap-menu.element.js';
import { CellSelection, TableMap, TableView } from '@tiptap/pm/tables';
import { Plugin } from '@tiptap/pm/state';
import type { EditorView, ViewMutationRecord } from '@tiptap/pm/view';
import type { PluginView, Selection, Transaction } from '@tiptap/pm/state';
import type { Node as ProseMirrorNode, ResolvedPos } from '@tiptap/pm/model';
import type { Rect } from '@tiptap/pm/tables';
import type { UUIPopoverContainerElement } from '@umbraco-cms/backoffice/external/uui';

// NOTE: Custom TableView with enhanced container structure for better popover handling. [LK]
export class UmbTableView extends TableView {
	#blockContainer: HTMLDivElement;
	#innerTableContainer: HTMLDivElement;
	#widgetsContainer: HTMLDivElement;
	#overlayContainer: HTMLDivElement;

	constructor(node: ProseMirrorNode, cellMinWidth: number) {
		super(node, cellMinWidth);
		this.#blockContainer = this.#createBlockContainer();
		this.#innerTableContainer = this.#createInnerTableContainer();
		this.#widgetsContainer = this.#createWidgetsContainer();
		this.#overlayContainer = this.#createOverlayContainer();
		this.#setupDOMStructure();
		this.#updateTableStyle(node);
	}

	#createBlockContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.setAttribute('data-content-type', 'table');
		return container;
	}

	#createInnerTableContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.className = 'table-container';
		return container;
	}

	#createWidgetsContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.className = 'table-controls';
		return container;
	}

	#createOverlayContainer(): HTMLDivElement {
		const container = document.createElement('div');
		container.className = 'table-selection-overlay';
		return container;
	}

	#setupDOMStructure(): void {
		// The parent TableView creates: <div class="tableWrapper"><table>...</table></div>
		// We restructure to: blockContainer > tableWrapper > innerTableContainer + widgetsContainer + overlayContainer
		const tableWrapper = this.dom as HTMLElement;
		const tableElement = tableWrapper.firstChild!;

		// Move table into inner container
		this.#innerTableContainer.appendChild(tableElement);

		// Build the hierarchy
		tableWrapper.appendChild(this.#innerTableContainer);
		tableWrapper.appendChild(this.#widgetsContainer);
		tableWrapper.appendChild(this.#overlayContainer);
		this.#blockContainer.appendChild(tableWrapper);

		// Update the DOM reference
		this.dom = this.#blockContainer;
	}

	// Get the widgets container for appending grips and popovers
	get widgetsContainer(): HTMLDivElement {
		return this.#widgetsContainer;
	}

	// Get the overlay container for selection UI
	get overlayContainer(): HTMLDivElement {
		return this.#overlayContainer;
	}

	override update(node: ProseMirrorNode): boolean {
		if (!super.update(node)) return false;
		this.#updateTableStyle(node);
		return true;
	}

	override ignoreMutation(mutation: ViewMutationRecord): boolean {
		// Only track mutations inside the table-container (the actual table content)
		// Ignore mutations in widgets and overlay containers
		const target = mutation.target as HTMLElement;
		const isInsideTable = target.closest('.table-container');
		return !isInsideTable || super.ignoreMutation(mutation);
	}

	#updateTableStyle(node: ProseMirrorNode) {
		if (node.attrs.style) {
			// NOTE: The `min-width` inline style is handled by the Tiptap TableView, so we need to preserve it. [LK]
			const minWidth = this.table.style.minWidth;
			const styles = node.attrs.style as string;
			this.table.style.cssText = `${styles}; min-width: ${minWidth};`;
		}
	}
}

interface TableHandlesState {
	show: boolean;
	rowIndex: number | undefined;
	colIndex: number | undefined;
	referencePosCell: DOMRect | undefined;
	referencePosTable: DOMRect | undefined;
	tableNode: ProseMirrorNode | undefined;
	tablePos: number | undefined;
	widgetContainer: HTMLElement | undefined;
}

/**
 * Plugin view that manages table grips and shared popover menu.
 * Consolidates grip management from header/cell plugins into a single location.
 */
class TableHandlePluginView implements PluginView {
	#editor: Editor;
	#view: EditorView;
	#state: TableHandlesState | undefined;
	#mouseState: 'up' | 'down' | 'selecting' = 'up';

	#rowGrip: HTMLElement | null = null;
	#colGrip: HTMLElement | null = null;
	#rowPopover: UUIPopoverContainerElement | null = null;
	#colPopover: UUIPopoverContainerElement | null = null;
	#rowMenu: UmbTiptapMenuElement | null = null;
	#colMenu: UmbTiptapMenuElement | null = null;
	#currentWidgetContainer: HTMLElement | null = null;

	constructor(editor: Editor, view: EditorView) {
		this.#editor = editor;
		this.#view = view;

		// Setup event listeners
		this.#view.dom.addEventListener('mousemove', this.#handleMouseMove);
		this.#view.dom.addEventListener('mousedown', this.#handleMouseDown);
		window.addEventListener('mouseup', this.#handleMouseUp);
	}

	#handleMouseMove = (event: MouseEvent) => {
		if (this.#mouseState === 'selecting') return;

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

	#handleMouseDown = () => {
		this.#mouseState = 'down';
	};

	#handleMouseUp = () => {
		this.#mouseState = 'up';
	};

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

		// Get the widget container from the UmbTableView
		const tableWrapper = cellElement.closest('.tableWrapper');
		const widgetContainer = tableWrapper?.querySelector('.table-controls') as HTMLElement | undefined;

		if (!widgetContainer) {
			this.#hideGrips();
			return;
		}

		// Get cell indices
		const cellIndices = this.#getCellIndicesFromDOM(cellElement, tableInfo.node);
		if (!cellIndices) {
			this.#hideGrips();
			return;
		}

		const tableBody = cellElement.closest('tbody') || cellElement.closest('table');
		const tableRect = tableBody?.getBoundingClientRect();
		const cellRect = cellElement.getBoundingClientRect();

		this.#state = {
			show: true,
			rowIndex: cellIndices.rowIndex,
			colIndex: cellIndices.colIndex,
			referencePosCell: cellRect,
			referencePosTable: tableRect,
			tableNode: tableInfo.node,
			tablePos: tableInfo.pos,
			widgetContainer,
		};

		this.#updateGrips(widgetContainer);
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
		if (!blockContainer?.hasAttribute('data-content-type')) return null;

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

	#getCellIndicesFromDOM(
		cell: HTMLTableCellElement,
		_tableNode: ProseMirrorNode,
	): { rowIndex: number; colIndex: number } | null {
		const row = cell.parentElement as HTMLTableRowElement | null;
		if (!row) return null;

		const tbody = row.parentElement;
		if (!tbody) return null;

		const rowIndex = Array.from(tbody.children).indexOf(row);
		const colIndex = Array.from(row.children).indexOf(cell);

		if (rowIndex === -1 || colIndex === -1) return null;

		return { rowIndex, colIndex };
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
		if (this.#currentWidgetContainer !== container) {
			this.#cleanupGrips();
			this.#createGrips(container);
			this.#currentWidgetContainer = container;
		}

		this.#positionGrips();
	}

	#createGrips(container: HTMLElement) {
		// Generate unique IDs for each popover
		const timestamp = Date.now();
		const rowPopoverId = `table-row-menu-${timestamp}`;
		const colPopoverId = `table-col-menu-${timestamp}`;

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
		if (!this.#state) return;

		const index = context === 'row' ? this.#state.rowIndex : this.#state.colIndex;
		if (index === undefined) return;

		// Select the row or column
		const tr = this.#editor.state.tr;
		const selectFn = context === 'row' ? selectRow : selectColumn;
		this.#editor.view.dispatch(selectFn(index)(tr));

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
		if (!this.#state?.referencePosCell || !this.#state?.referencePosTable) return;

		const { referencePosCell, referencePosTable } = this.#state;

		// The .table-controls container is offset by -1rem from the table
		// We need to add 1rem (16px) to compensate for this offset
		const containerOffset = 16; // 1rem in pixels

		// Position row grip (left side of cell)
		if (this.#rowGrip) {
			this.#rowGrip.style.display = 'flex';
			this.#rowGrip.style.top = `${referencePosCell.top - referencePosTable.top + containerOffset}px`;
			this.#rowGrip.style.height = `${referencePosCell.height}px`;
		}

		// Position column grip (top of cell)
		if (this.#colGrip) {
			this.#colGrip.style.display = 'flex';
			this.#colGrip.style.left = `${referencePosCell.left - referencePosTable.left + containerOffset}px`;
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
		this.#view.dom.removeEventListener('mousedown', this.#handleMouseDown);
		window.removeEventListener('mouseup', this.#handleMouseUp);
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

export const UmbTable = Table.extend({
	addProseMirrorPlugins() {
		const plugins = this.parent?.() ?? [];
		// Add our TableHandlePlugin for managing grips and context menus
		plugins.push(TableHandlePlugin(this.editor));
		return plugins;
	},
}).configure({
	resizable: true,
	View: UmbTableView,
});

export const UmbTableRow = TableRow.extend({
	allowGapCursor: false,
	content: '(tableCell | tableHeader)*',
});

export const UmbTableHeader = TableHeader.extend({
	addAttributes() {
		return {
			colspan: {
				default: 1,
			},
			rowspan: {
				default: 1,
			},
			colwidth: {
				default: null,
				parseHTML: (element) => {
					const colwidth = element.getAttribute('colwidth');
					const value = colwidth ? colwidth.split(',').map((item) => parseInt(item, 10)) : null;

					return value;
				},
			},
			style: {
				default: null,
			},
		};
	},
	// NOTE: Grip decorations and bubble menus are now handled by TableHandlePlugin [LK]
});

export const UmbTableCell = TableCell.extend({
	addAttributes() {
		return {
			colspan: {
				default: 1,
				parseHTML: (element) => {
					const colspan = element.getAttribute('colspan');
					const value = colspan ? parseInt(colspan, 10) : 1;

					return value;
				},
			},
			rowspan: {
				default: 1,
				parseHTML: (element) => {
					const rowspan = element.getAttribute('rowspan');
					const value = rowspan ? parseInt(rowspan, 10) : 1;

					return value;
				},
			},
			colwidth: {
				default: null,
				parseHTML: (element) => {
					const colwidth = element.getAttribute('colwidth');
					const value = colwidth ? [parseInt(colwidth, 10)] : null;

					return value;
				},
			},
			style: {
				default: null,
			},
		};
	},
	// NOTE: Grip decorations and bubble menus are now handled by TableHandlePlugin [LK]
});

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

const findTable = (selection: Selection) =>
	findParentNode((node) => node.type.spec.tableRole && node.type.spec.tableRole === 'table')(selection);

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

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const isTableSelected = (selection: Selection) => {
	if (isCellSelection(selection)) {
		const map = TableMap.get(selection.$anchorCell.node(-1));

		return isRectSelected({
			left: 0,
			right: map.width,
			top: 0,
			bottom: map.height,
		})(selection);
	}

	return false;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getCellsInColumn = (columnIndex: number | number[]) => (selection: Selection) => {
	const table = findTable(selection);
	if (table) {
		const map = TableMap.get(table.node);
		const indexes = Array.isArray(columnIndex) ? columnIndex : Array.from([columnIndex]);

		return indexes.reduce(
			(acc, index) => {
				if (index >= 0 && index <= map.width - 1) {
					const cells = map.cellsInRect({
						left: index,
						right: index + 1,
						top: 0,
						bottom: map.height,
					});

					return acc.concat(
						cells.map((nodePos) => {
							const node = table.node.nodeAt(nodePos);
							const pos = nodePos + table.start;

							return { pos, start: pos + 1, node };
						}),
					);
				}

				return acc;
			},
			[] as { pos: number; start: number; node: ProseMirrorNode | null | undefined }[],
		);
	}
	return null;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getCellsInRow = (rowIndex: number | number[]) => (selection: Selection) => {
	const table = findTable(selection);

	if (table) {
		const map = TableMap.get(table.node);
		const indexes = Array.isArray(rowIndex) ? rowIndex : Array.from([rowIndex]);

		return indexes.reduce(
			(acc, index) => {
				if (index >= 0 && index <= map.height - 1) {
					const cells = map.cellsInRect({
						left: 0,
						right: map.width,
						top: index,
						bottom: index + 1,
					});

					return acc.concat(
						cells.map((nodePos) => {
							const node = table.node.nodeAt(nodePos);
							const pos = nodePos + table.start;
							return { pos, start: pos + 1, node };
						}),
					);
				}

				return acc;
			},
			[] as { pos: number; start: number; node: ProseMirrorNode | null | undefined }[],
		);
	}

	return null;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const getCellsInTable = (selection: Selection) => {
	const table = findTable(selection);

	if (table) {
		const map = TableMap.get(table.node);
		const cells = map.cellsInRect({
			left: 0,
			right: map.width,
			top: 0,
			bottom: map.height,
		});

		return cells.map((nodePos) => {
			const node = table.node.nodeAt(nodePos);
			const pos = nodePos + table.start;

			return { pos, start: pos + 1, node };
		});
	}

	return null;
};

const findParentNodeClosestToPos = ($pos: ResolvedPos, predicate: (node: ProseMirrorNode) => boolean) => {
	for (let i = $pos.depth; i > 0; i -= 1) {
		const node = $pos.node(i);

		if (predicate(node)) {
			return {
				pos: i > 0 ? $pos.before(i) : 0,
				start: $pos.start(i),
				depth: i,
				node,
			};
		}
	}

	return null;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const findCellClosestToPos = ($pos: ResolvedPos) => {
	const predicate = (node: ProseMirrorNode) => node.type.spec.tableRole && /cell/i.test(node.type.spec.tableRole);

	return findParentNodeClosestToPos($pos, predicate);
};

const select = (type: 'row' | 'column') => (index: number) => (tr: Transaction) => {
	const table = findTable(tr.selection);
	const isRowSelection = type === 'row';

	if (table) {
		const map = TableMap.get(table.node);

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

			const head = table.start + cellsInFirstRow[0];
			const anchor = table.start + cellsInLastRow[cellsInLastRow.length - 1];
			const $head = tr.doc.resolve(head);
			const $anchor = tr.doc.resolve(anchor);

			return tr.setSelection(new CellSelection($anchor, $head));
		}
	}
	return tr;
};

const selectColumn = select('column');

const selectRow = select('row');
