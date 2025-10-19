import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextAlignExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];
}
