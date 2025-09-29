import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TextIndent } from './text-indent.tiptap-extension.js';

export default class UmbTiptapTextIndentExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextIndent.configure({
			types: ['div', 'heading', 'paragraph', 'blockquote', 'listItem', 'orderedList', 'bulletList'],
		}),
	];
}
