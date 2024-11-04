import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextAlignLeftExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'left' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('left').run();
	}
}
