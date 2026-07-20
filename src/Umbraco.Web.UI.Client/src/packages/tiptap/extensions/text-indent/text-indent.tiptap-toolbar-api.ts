import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarTextIndentExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		if (!editor) return;

		// If inside a list, sink the list item instead of adding indent
		if (editor.can().sinkListItem('listItem')) {
			editor.chain().focus().sinkListItem('listItem').run();
		} else if (!editor.isActive('listItem')) {
			editor.chain().focus().textIndent().run();
		}
	}
}
