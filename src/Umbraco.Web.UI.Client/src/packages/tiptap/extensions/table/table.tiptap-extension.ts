import { Table, TableCell, TableHeader, TableRow } from '../../externals.js';
import { TableHandlePlugin } from './plugins/table-handle.js';
import { UmbTableView } from './plugins/table-view.js';

export const UmbTable = Table.extend({
	addProseMirrorPlugins() {
		return [...(this.parent?.() ?? []), TableHandlePlugin(this.editor)];
	},
	addNodeView() {
		return ({ node, HTMLAttributes }) => {
			return new UmbTableView(node, this.options.cellMinWidth, HTMLAttributes);
		};
	},
}).configure({
	resizable: true,
});

export const UmbTableRow = TableRow.extend();
export const UmbTableHeader = TableHeader.extend();
export const UmbTableCell = TableCell.extend();

export { UmbTableView };
