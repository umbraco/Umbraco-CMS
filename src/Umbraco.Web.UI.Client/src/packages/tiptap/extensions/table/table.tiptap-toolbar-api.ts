import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export type UmbTiptapToolbarTableMenuItem = {
	label: string;
	icon?: string;
	execute: (editor: Editor) => void;
};

export class UmbTiptapToolbarTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	public getMenu = (): Array<UmbTiptapToolbarTableMenuItem> => [
		{
			label: 'Insert table',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().insertTable({ rows: 2, cols: 2, withHeaderRow: false }).run(),
		},
		{
			label: 'Add column before',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addColumnBefore().run(),
		},
		{
			label: 'Add column after',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addColumnAfter().run(),
		},
		{
			label: 'Delete column',
			icon: 'icon-trash',
			execute: (editor: Editor) => editor.chain().focus().deleteColumn().run(),
		},
		{
			label: 'Add row before',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addRowBefore().run(),
		},
		{
			label: 'Add row after',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().addRowAfter().run(),
		},
		{
			label: 'Delete row',
			icon: 'icon-trash',
			execute: (editor: Editor) => editor.chain().focus().deleteRow().run(),
		},
		{
			label: 'Delete table',
			icon: 'icon-trash',
			execute: (editor: Editor) => editor.chain().focus().deleteTable().run(),
		},
		{
			label: 'Merge cells',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().mergeCells().run(),
		},
		{
			label: 'Split cell',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().splitCell().run(),
		},
		{
			label: 'Toggle header column',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().toggleHeaderColumn().run(),
		},
		{
			label: 'Toggle header row',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().toggleHeaderRow().run(),
		},
		{
			label: 'Toggle header cell',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().toggleHeaderCell().run(),
		},
		{
			label: 'Merge or split',
			icon: 'icon-table',
			execute: (editor: Editor) => editor.chain().focus().mergeOrSplit().run(),
		},
	];
}

export { UmbTiptapToolbarTableExtensionApi as api };
