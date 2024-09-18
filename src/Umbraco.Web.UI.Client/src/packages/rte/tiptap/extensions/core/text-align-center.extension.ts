import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.TextAlignCenter',
	name: 'Text Align Center Tiptap Extension',
	api: () => import('./text-align-center.extension.js'),
	weight: 918,
	meta: {
		alias: 'text-align-center',
		icon: 'text-align-center',
		label: 'Text Align Center',
	},
};

export default class UmbTiptapTextAlignCenterPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('center').run();
	}
}
