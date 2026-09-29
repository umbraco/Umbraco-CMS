import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapToolbarTextDirectionLtrExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive({ dir: 'ltr' }) === true || editor?.isActive({ dir: 'auto' }) === true;
	}

	override execute(editor?: Editor) {
		if (!this.isActive(editor)) {
			editor?.chain().focus().setTextDirection('ltr').run();
		} else {
			editor?.chain().focus().unsetTextDirection().run();
		}
	}
}
