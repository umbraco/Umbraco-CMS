import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TextAlign } from '../../externals.js';

export default class UmbTiptapTextAlignExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];
}
