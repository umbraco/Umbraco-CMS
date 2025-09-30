import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';

export default class UmbTiptapToolbarUnlinkExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive = (editor?: Editor) => editor?.isActive('umbLink') === true;

	override isDisabled = (editor?: Editor) => !this.isActive(editor);

	override execute(editor?: Editor) {
		editor?.chain().focus().unsetUmbLink().run();
	}
}
