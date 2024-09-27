import { UmbTiptapExtensionApiBase } from '../types.js';
import { Table, TableHeader, TableRow, TableCell } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTableExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Table.configure({ resizable: true }), TableHeader, TableRow, TableCell];
}
