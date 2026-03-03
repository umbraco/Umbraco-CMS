import { UmbTiptapToolbarActionButtonApiBase } from '../tiptap-toolbar-action-button-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarRedoExtensionApi extends UmbTiptapToolbarActionButtonApiBase {
	override isActive = (editor?: Editor) => editor?.can().redo() === true;

	override execute(editor?: Editor) {
		editor?.chain().focus().redo().run();
	}
}
