import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Code, CodeBlock } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapCodeBlockExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Code, CodeBlock];

	override execute(editor?: Editor) {
		// editor.chain().focus().toggleCode().run();
		editor?.chain().focus().toggleCodeBlock().run();
	}
}
