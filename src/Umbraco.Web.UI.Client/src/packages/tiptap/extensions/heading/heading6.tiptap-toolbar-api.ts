import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';

export default class UmbTiptapToolbarHeading6ExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive('heading', { level: 6 }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 6 }).run();
	}
}
