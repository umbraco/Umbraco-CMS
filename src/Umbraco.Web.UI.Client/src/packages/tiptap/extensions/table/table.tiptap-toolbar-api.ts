import type { UmbCascadingMenuItem } from '../../components/cascading-menu-popover/cascading-menu-popover.element.js';
import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { UmbTiptapTableInsertElement } from './components/table-insert.element.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapToolbarTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	public override execute() {}

	public getMenu(editor?: Editor): Array<UmbCascadingMenuItem> {
		const tableInsertElement = new UmbTiptapTableInsertElement();
		tableInsertElement.editor = editor;

		return [
			{
				unique: 'table-menu-table',
				label: 'Table',
				icon: 'icon-table',
				items: [
					{
						unique: 'table-insert',
						label: 'Insert table',
						element: tableInsertElement,
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
						execute: () => editor?.chain().focus().mergeCells().run(),
					},
					{
						unique: 'table-split',
						label: 'Split cell',
						execute: () => editor?.chain().focus().splitCell().run(),
					},
					{
						unique: 'table-merge-split',
						label: 'Merge or split',
						execute: () => editor?.chain().focus().mergeOrSplit().run(),
					},
					{
						unique: 'table-header-cell',
						label: 'Toggle header cell',
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
						execute: () => editor?.chain().focus().addRowBefore().run(),
					},
					{
						unique: 'table-row-after',
						label: 'Add row after',
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
						execute: () => editor?.chain().focus().addColumnBefore().run(),
					},
					{
						unique: 'table-column-after',
						label: 'Add column after',
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
}

export { UmbTiptapToolbarTableExtensionApi as api };
