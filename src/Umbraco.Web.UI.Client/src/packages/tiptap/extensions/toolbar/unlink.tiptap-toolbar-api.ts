import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarUnlinkExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive = (editor?: Editor) => editor?.isActive('umbLink') ?? false;

	override execute(editor?: Editor) {
		editor?.chain().focus().unsetUmbLink().run();
	}
}
