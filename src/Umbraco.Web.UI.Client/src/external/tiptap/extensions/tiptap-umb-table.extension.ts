import { CellSelection, TableMap } from '@tiptap/pm/tables';
import { Decoration, DecorationSet } from '@tiptap/pm/view';
import { EditorState } from '@tiptap/pm/state';
import { EditorView } from '@tiptap/pm/view';
import { findParentNode, mergeAttributes, Editor, Node } from '@tiptap/core';
import { Node as PMNode, ResolvedPos } from '@tiptap/pm/model';
import { Plugin } from '@tiptap/pm/state';
import { Selection, Transaction } from '@tiptap/pm/state';
import { Table } from '@tiptap/extension-table';
import { TableCell } from '@tiptap/extension-table-cell';
import { TableHeader } from '@tiptap/extension-table-header';
import { TableRow } from '@tiptap/extension-table-row';
import type { Rect } from '@tiptap/pm/tables';

export const UmbTable = Table.configure({ resizable: true });

export const UmbTableRow = TableRow.extend({
	allowGapCursor: false,
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

	addProseMirrorPlugins() {
		return [
			new Plugin({
				props: {
					decorations: (state) => {
						const { isEditable } = this.editor;

						if (!isEditable) {
							return DecorationSet.empty;
						}

						const { doc, selection } = state;
						const decorations: Array<Decoration> = [];
						const cells = getCellsInRow(0)(selection);

						if (cells) {
							cells.forEach(({ pos }: { pos: number }, index: number) => {
								decorations.push(
									Decoration.widget(pos + 1, () => {
										const colSelected = isColumnSelected(index)(selection);
										const className = colSelected ? 'grip-column selected' : 'grip-column';

										const grip = document.createElement('a');
										grip.appendChild(document.createElement('uui-symbol-more'));

										grip.className = className;
										grip.addEventListener('mousedown', (event) => {
											event.preventDefault();
											event.stopImmediatePropagation();

											this.editor.view.dispatch(selectColumn(index)(this.editor.state.tr));
										});

										return grip;
									}),
								);
							});
						}

						return DecorationSet.create(doc, decorations);
					},
				},
			}),
		];
	},
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

	addProseMirrorPlugins() {
		return [
			new Plugin({
				props: {
					decorations: (state) => {
						const { isEditable } = this.editor;

						if (!isEditable) {
							return DecorationSet.empty;
						}

						const { doc, selection } = state;
						const decorations: Decoration[] = [];
						const cells = getCellsInColumn(0)(selection);

						if (cells) {
							cells.forEach(({ pos }: { pos: number }, index: number) => {
								decorations.push(
									Decoration.widget(pos + 1, () => {
										const rowSelected = isRowSelected(index)(selection);
										const className = rowSelected ? 'grip-row selected' : 'grip-row';

										const grip = document.createElement('a');
										grip.appendChild(document.createElement('uui-symbol-more'));

										grip.className = className;
										grip.addEventListener('mousedown', (event) => {
											event.preventDefault();
											event.stopImmediatePropagation();

											this.editor.view.dispatch(selectRow(index)(this.editor.state.tr));
										});

										return grip;
									}),
								);
							});
						}

						return DecorationSet.create(doc, decorations);
					},
				},
			}),
		];
	},
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
			[] as { pos: number; start: number; node: PMNode | null | undefined }[],
		);
	}
	return null;
};

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
			[] as { pos: number; start: number; node: PMNode | null | undefined }[],
		);
	}

	return null;
};

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

const findParentNodeClosestToPos = ($pos: ResolvedPos, predicate: (node: PMNode) => boolean) => {
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

const findCellClosestToPos = ($pos: ResolvedPos) => {
	const predicate = (node: PMNode) => node.type.spec.tableRole && /cell/i.test(node.type.spec.tableRole);

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

const selectTable = (tr: Transaction) => {
	const table = findTable(tr.selection);

	if (table) {
		const { map } = TableMap.get(table.node);

		if (map && map.length) {
			const head = table.start + map[0];
			const anchor = table.start + map[map.length - 1];
			const $head = tr.doc.resolve(head);
			const $anchor = tr.doc.resolve(anchor);

			return tr.setSelection(new CellSelection($anchor, $head));
		}
	}

	return tr;
};

const isColumnGripSelected = ({
	editor,
	view,
	state,
	from,
}: {
	editor: Editor;
	view: EditorView;
	state: EditorState;
	from: number;
}) => {
	const domAtPos = view.domAtPos(from).node as HTMLElement;
	const nodeDOM = view.nodeDOM(from) as HTMLElement;
	const node = nodeDOM || domAtPos;

	if (!editor.isActive(Table.name) || !node || isTableSelected(state.selection)) {
		return false;
	}

	let container = node;

	while (container && !['TD', 'TH'].includes(container.tagName)) {
		container = container.parentElement!;
	}

	const gripColumn = container && container.querySelector && container.querySelector('a.grip-column.selected');

	return !!gripColumn;
};

export const isRowGripSelected = ({
	editor,
	view,
	state,
	from,
}: {
	editor: Editor;
	view: EditorView;
	state: EditorState;
	from: number;
}) => {
	const domAtPos = view.domAtPos(from).node as HTMLElement;
	const nodeDOM = view.nodeDOM(from) as HTMLElement;
	const node = nodeDOM || domAtPos;

	if (!editor.isActive(Table.name) || !node || isTableSelected(state.selection)) {
		return false;
	}

	let container = node;

	while (container && !['TD', 'TH'].includes(container.tagName)) {
		container = container.parentElement!;
	}

	const gripRow = container && container.querySelector && container.querySelector('a.grip-row.selected');

	return !!gripRow;
};
