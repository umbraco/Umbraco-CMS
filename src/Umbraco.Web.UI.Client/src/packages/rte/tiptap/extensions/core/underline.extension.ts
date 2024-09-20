import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Underline } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapUnderlineExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Underline];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleUnderline().run();
	}
}
