import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarTextOutdentExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		if (!editor) return;

		// If inside a list, lift the list item instead of removing indent
		if (editor.can().liftListItem('listItem')) {
			editor.chain().focus().liftListItem('listItem').run();
		} else if (!editor.isActive('listItem')) {
			editor.chain().focus().textOutdent().run();
		}
	}
}
