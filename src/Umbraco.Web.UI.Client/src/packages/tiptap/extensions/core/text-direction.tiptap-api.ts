import { UmbTiptapExtensionApiBase } from '../base.js';
import { TextDirection } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextDirectionExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextDirection.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList'],
		}),
	];
}
