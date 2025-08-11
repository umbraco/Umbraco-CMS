import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export class UmbTiptapToolbarTableExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor, item?: unknown) {
		if (!item) return super.isActive(editor);
		return false;
	}

	override execute() {}
}

export { UmbTiptapToolbarTableExtensionApi as api };
