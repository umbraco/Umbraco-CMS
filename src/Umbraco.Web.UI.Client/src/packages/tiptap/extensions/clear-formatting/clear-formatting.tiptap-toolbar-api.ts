import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarClearFormattingExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		editor?.chain().focus().clearNodes().unsetAllMarks().unsetClassName().unsetStyles().run();
	}
}
