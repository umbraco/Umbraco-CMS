import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Table, TableHeader, TableRow, TableCell } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Table.configure({ resizable: true }), TableHeader, TableRow, TableCell];

	override execute(editor?: Editor) {
		editor?.commands.insertTable({ rows: 3, cols: 3, withHeaderRow: true });
	}
}
