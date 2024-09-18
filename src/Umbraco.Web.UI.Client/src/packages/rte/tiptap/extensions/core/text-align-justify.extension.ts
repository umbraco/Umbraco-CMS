import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.TextAlignJustify',
	name: 'Text Align Justify Tiptap Extension',
	api: () => import('./text-align-justify.extension.js'),
	weight: 916,
	meta: {
		alias: 'text-align-justify',
		icon: 'text-align-justify',
		label: 'Text Align Justify',
	},
};

export default class UmbTiptapTextAlignJustifyPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('justify').run();
	}
}
