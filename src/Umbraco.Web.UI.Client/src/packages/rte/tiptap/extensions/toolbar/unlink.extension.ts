import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapUnlinkExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive = (editor?: Editor) => editor?.isActive('umbLink') ?? false;

	override execute(editor?: Editor) {
		editor?.chain().focus().unsetUmbLink().run();
	}
}
