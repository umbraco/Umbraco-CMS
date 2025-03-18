import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { MetaTiptapToolbarMenuItem } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapToolbarTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	#commands: Record<string, (editor?: Editor) => void> = {
		mergeCells: (editor) => editor?.chain().focus().mergeCells().run(),
		splitCell: (editor) => editor?.chain().focus().splitCell().run(),
		mergeOrSplit: (editor) => editor?.chain().focus().mergeOrSplit().run(),
		toggleHeaderCell: (editor) => editor?.chain().focus().toggleHeaderCell().run(),
		addRowBefore: (editor) => editor?.chain().focus().addRowBefore().run(),
		addRowAfter: (editor) => editor?.chain().focus().addRowAfter().run(),
		deleteRow: (editor) => editor?.chain().focus().deleteRow().run(),
		toggleHeaderRow: (editor) => editor?.chain().focus().toggleHeaderRow().run(),
		addColumnBefore: (editor) => editor?.chain().focus().addColumnBefore().run(),
		addColumnAfter: (editor) => editor?.chain().focus().addColumnAfter().run(),
		deleteColumn: (editor) => editor?.chain().focus().deleteColumn().run(),
		toggleHeaderColumn: (editor) => editor?.chain().focus().toggleHeaderColumn().run(),
		deleteTable: (editor) => editor?.chain().focus().deleteTable().run(),
	};

	override execute(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		if (!item?.data) return;
		const key = item.data.toString();
		this.#commands[key](editor);
	}
}

export { UmbTiptapToolbarTableExtensionApi as api };
