import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextAlignCenterExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'center' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('center').run();
	}
}
