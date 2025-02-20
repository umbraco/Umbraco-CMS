import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarRedoExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor: Editor): boolean {
		return editor.can().redo();
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().redo().run();
	}
}
