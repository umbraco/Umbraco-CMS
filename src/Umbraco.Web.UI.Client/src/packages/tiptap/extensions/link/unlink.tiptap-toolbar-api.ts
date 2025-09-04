import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarUnlinkExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive = (editor?: Editor) => editor?.isActive('umbLink') === true;

	override isDisabled = (editor?: Editor) => !this.isActive(editor);

	override execute(editor?: Editor) {
		editor?.chain().focus().unsetUmbLink().run();
	}
}
