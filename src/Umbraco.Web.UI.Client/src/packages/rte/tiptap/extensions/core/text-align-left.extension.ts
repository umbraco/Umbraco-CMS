import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.TextAlignLeft',
	name: 'Text Align Left Tiptap Extension',
	api: () => import('./text-align-left.extension.js'),
	weight: 919,
	meta: {
		alias: 'text-align-left',
		icon: 'text-align-left',
		label: 'Text Align Left',
	},
};

export default class UmbTiptapTextAlignLeftPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('left').run();
	}
}
