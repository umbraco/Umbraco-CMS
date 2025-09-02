import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TextDirection } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextDirectionExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		TextDirection.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList'],
		}),
	];
}
