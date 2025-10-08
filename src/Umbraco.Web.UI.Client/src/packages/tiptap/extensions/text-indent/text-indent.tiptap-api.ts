import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TextIndent } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextIndentExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		// eslint-disable-next-line @typescript-eslint/no-deprecated
		TextIndent.configure({
			types: ['div', 'heading', 'paragraph', 'blockquote', 'listItem', 'orderedList', 'bulletList'],
		}),
	];
}
