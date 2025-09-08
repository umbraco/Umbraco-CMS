import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarHeading1ExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive('heading', { level: 1 }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 1 }).run();
	}
}
