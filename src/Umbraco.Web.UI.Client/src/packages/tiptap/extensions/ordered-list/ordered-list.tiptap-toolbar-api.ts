import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { OrderedList, ListItem } from '../../externals.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarOrderedListExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [OrderedList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleOrderedList().run();
	}
}
