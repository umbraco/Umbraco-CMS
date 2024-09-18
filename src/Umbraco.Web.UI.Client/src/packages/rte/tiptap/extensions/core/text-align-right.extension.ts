import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.TextAlignRight',
	name: 'Text Align Right Tiptap Extension',
	api: () => import('./text-align-right.extension.js'),
	weight: 917,
	meta: {
		alias: 'text-align-right',
		icon: 'text-align-right',
		label: 'Text Align Right',
	},
};

export default class UmbTiptapTextAlignRightPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('right').run();
	}
}
