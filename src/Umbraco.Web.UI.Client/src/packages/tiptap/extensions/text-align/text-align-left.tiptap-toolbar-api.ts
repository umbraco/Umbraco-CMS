import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextAlignLeftExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'left' }) === true;
	}

	override execute(editor?: Editor) {
		if (!this.isActive(editor)) {
			editor?.chain().focus().setTextAlign('left').run();
		} else {
			editor?.chain().focus().unsetTextAlign().run();
		}
	}
}
