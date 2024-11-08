import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextAlignRightExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'right' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('right').run();
	}
}
