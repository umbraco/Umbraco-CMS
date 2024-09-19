import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
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

export default class UmbTiptapTextAlignCenterExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'center' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('center').run();
	}
}
