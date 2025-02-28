import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { MetaTiptapToolbarMenuItem } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarStyleSelectExtensionApi extends UmbTiptapToolbarElementApiBase {
	#commands: Record<string, (editor?: Editor) => void> = {
		h2: (editor) => editor?.chain().focus().toggleHeading({ level: 2 }).run(),
		h3: (editor) => editor?.chain().focus().toggleHeading({ level: 3 }).run(),
		h4: (editor) => editor?.chain().focus().toggleHeading({ level: 4 }).run(),
		p: (editor) => editor?.chain().focus().setParagraph().run(),
		blockquote: (editor) => editor?.chain().focus().toggleBlockquote().run(),
		codeBlock: (editor) => editor?.chain().focus().toggleCodeBlock().run(),
	};

	override execute(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		if (!item?.data) return;
		const key = item.data.toString();
		this.#commands[key](editor);
	}
}
