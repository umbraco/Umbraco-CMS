import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarCodeBlockExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		// editor.chain().focus().toggleCode().run();
		editor?.chain().focus().toggleCodeBlock().run();
	}
}
