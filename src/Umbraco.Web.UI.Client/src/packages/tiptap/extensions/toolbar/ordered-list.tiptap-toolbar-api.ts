import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { OrderedList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarOrderedListExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [OrderedList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleOrderedList().run();
	}
}
