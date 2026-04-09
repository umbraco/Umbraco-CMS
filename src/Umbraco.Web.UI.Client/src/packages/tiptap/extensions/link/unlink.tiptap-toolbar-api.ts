import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarActionButtonApiBase } from '../tiptap-toolbar-action-button-api-base.js';

export default class UmbTiptapToolbarUnlinkExtensionApi extends UmbTiptapToolbarActionButtonApiBase {
	override isActive = (editor?: Editor) => editor?.isActive('umbLink') === true;

	override execute(editor?: Editor) {
		editor?.chain().focus().unsetUmbLink().run();
	}
}
