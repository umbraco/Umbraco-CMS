import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextAlignRightExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'right' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('right').run();
	}
}
