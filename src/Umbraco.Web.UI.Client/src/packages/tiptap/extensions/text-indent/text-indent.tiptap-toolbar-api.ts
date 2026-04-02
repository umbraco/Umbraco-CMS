import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarTextIndentExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		editor?.chain().focus().textIndent().run();
	}
}
