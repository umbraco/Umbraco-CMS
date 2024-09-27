import { UmbTiptapExtensionApiBase } from '../types.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextAlignCenterExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];
}
