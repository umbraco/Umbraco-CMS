import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.TextAlignJustify',
	name: 'Text Align Justify Tiptap Extension',
	api: () => import('./text-align-justify.extension.js'),
	weight: 916,
	meta: {
		alias: 'text-align-justify',
		icon: 'icon-text-align-justify',
		label: 'Text Align Justify',
	},
};

export default class UmbTiptapTextAlignJustifyExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'justify' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('justify').run();
	}
}
