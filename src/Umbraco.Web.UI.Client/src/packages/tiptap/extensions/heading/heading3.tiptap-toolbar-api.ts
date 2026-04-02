import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';

export default class UmbTiptapToolbarHeading3ExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive('heading', { level: 3 }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 3 }).run();
	}
}
