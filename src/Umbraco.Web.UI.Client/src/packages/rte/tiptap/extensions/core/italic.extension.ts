import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Italic } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapItalicExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Italic];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleItalic().run();
	}
}
