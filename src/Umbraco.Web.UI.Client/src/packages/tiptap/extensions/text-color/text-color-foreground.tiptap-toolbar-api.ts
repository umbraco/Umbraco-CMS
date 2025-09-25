import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextColorForegroundExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor, selectedColor?: string) {
		editor?.chain().focus().toggleSpanStyle(`color: ${selectedColor};`).run();
	}
}
