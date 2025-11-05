import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';

export default class UmbTiptapToolbarCodeBlockExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		// editor.chain().focus().toggleCode().run();
		editor?.chain().focus().toggleCodeBlock().run();
	}
}
