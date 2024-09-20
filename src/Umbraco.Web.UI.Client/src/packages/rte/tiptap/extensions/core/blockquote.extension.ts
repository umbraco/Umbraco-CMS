import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Blockquote } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBlockquoteExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Blockquote];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBlockquote().run();
	}
}
