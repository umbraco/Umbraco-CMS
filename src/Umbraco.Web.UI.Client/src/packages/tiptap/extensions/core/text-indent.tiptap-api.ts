import { UmbTiptapExtensionApiBase } from '../base.js';
import { TextIndent } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTextIndentExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [
		TextIndent.configure({
			types: ['div', 'heading', 'paragraph', 'blockquote', 'listItem', 'orderedList', 'bulletList'],
		}),
	];
}
