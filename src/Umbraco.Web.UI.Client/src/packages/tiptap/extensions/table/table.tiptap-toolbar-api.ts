import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor, item?: unknown) {
		if (!item) return super.isActive(editor);
		return false;
	}

	override execute() {}
}
