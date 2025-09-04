import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextDirectionRtlExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive = (editor?: Editor) => editor?.isActive({ textDirection: 'rtl' }) === true;

	override execute(editor?: Editor) {
		if (!this.isActive(editor)) {
			editor?.chain().focus().setTextDirection('rtl').run();
		} else {
			editor?.chain().focus().unsetTextDirection().run();
		}
	}
}
