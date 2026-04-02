import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarTextColorBackgroundExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor, selectedColor?: string) {
		editor?.chain().focus().toggleSpanStyle(`background-color: ${selectedColor};`).run();
	}
}
