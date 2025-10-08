import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarTextDirectionLtrExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive = (editor?: Editor) =>
		editor?.isActive({ textDirection: 'ltr' }) === true || editor?.isActive({ textDirection: 'auto' }) === true;

	override execute(editor?: Editor) {
		if (!this.isActive(editor)) {
			editor?.chain().focus().setTextDirection('ltr').run();
		} else {
			editor?.chain().focus().unsetTextDirection().run();
		}
	}
}
