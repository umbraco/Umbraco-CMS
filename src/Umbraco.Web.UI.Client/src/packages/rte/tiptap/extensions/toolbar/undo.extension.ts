import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapUndoExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor: Editor): boolean {
		return editor.can().undo();
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().undo().run();
	}
}
