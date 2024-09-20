import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextAlignLeftExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'left' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('left').run();
	}
}
