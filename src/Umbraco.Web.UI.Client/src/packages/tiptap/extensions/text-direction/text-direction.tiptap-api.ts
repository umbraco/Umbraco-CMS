import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TextDirection } from './text-direction.tiptap-extension.js';

export default class UmbTiptapTextDirectionExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextDirection.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList'],
		}),
	];
}
