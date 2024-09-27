import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapRedoExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor: Editor): boolean {
		return editor.can().redo();
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().redo().run();
	}
}
