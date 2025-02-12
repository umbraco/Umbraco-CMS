import type { UmbCascadingMenuItem } from '../../components/cascading-menu-popover/cascading-menu-popover.element.js';
import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapToolbarTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	public getMenu = (editor?: Editor): Array<UmbCascadingMenuItem> => [
		{
			unique: 'table-menu-table',
			label: 'Table',
			icon: 'icon-table',
			items: [
				{
					unique: 'table-insert',
					label: 'Insert table',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().insertTable({ rows: 2, cols: 2, withHeaderRow: false }).run(),
					//element: document.createElement('uui-loader-circle'),
				},
			],
			separatorAfter: true,
		},
		{
			unique: 'table-menu-cell',
			label: 'Cell',
			items: [
				{
					unique: 'table-merge',
					label: 'Merge cells',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().mergeCells().run(),
				},
				{
					unique: 'table-split',
					label: 'Split cell',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().splitCell().run(),
				},
				{
					unique: 'table-merge-split',
					label: 'Merge or split',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().mergeOrSplit().run(),
				},
				{
					unique: 'table-header-cell',
					label: 'Toggle header cell',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().toggleHeaderCell().run(),
				},
			],
		},
		{
			unique: 'table-menu-row',
			label: 'Row',
			items: [
				{
					unique: 'table-row-before',
					label: 'Add row before',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().addRowBefore().run(),
				},
				{
					unique: 'table-row-after',
					label: 'Add row after',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().addRowAfter().run(),
				},
				{
					unique: 'table-row-delete',
					label: 'Delete row',
					icon: 'icon-trash',
					execute: () => editor?.chain().focus().deleteRow().run(),
				},
				{
					unique: 'table-header-row',
					label: 'Toggle header row',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().toggleHeaderRow().run(),
				},
			],
		},
		{
			unique: 'table-menu-column',
			label: 'Column',
			items: [
				{
					unique: 'table-column-before',
					label: 'Add column before',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().addColumnBefore().run(),
				},
				{
					unique: 'table-column-after',
					label: 'Add column after',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().addColumnAfter().run(),
				},
				{
					unique: 'table-column-delete',
					label: 'Delete column',
					icon: 'icon-trash',
					execute: () => editor?.chain().focus().deleteColumn().run(),
				},
				{
					unique: 'table-header-column',
					label: 'Toggle header column',
					icon: 'icon-table',
					execute: () => editor?.chain().focus().toggleHeaderColumn().run(),
				},
			],
			separatorAfter: true,
		},
		{
			unique: 'table-delete',
			label: 'Delete table',
			icon: 'icon-trash',
			execute: () => editor?.chain().focus().deleteTable().run(),
		},
	];
}

export { UmbTiptapToolbarTableExtensionApi as api };
