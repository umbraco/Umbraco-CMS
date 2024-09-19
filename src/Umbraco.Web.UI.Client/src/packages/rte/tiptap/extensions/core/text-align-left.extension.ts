import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { TextAlign } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.TextAlignLeft',
	name: 'Text Align Left Tiptap Extension',
	api: () => import('./text-align-left.extension.js'),
	weight: 919,
	meta: {
		alias: 'text-align-left',
		icon: 'icon-text-align-left',
		label: 'Text Align Left',
	},
};

export default class UmbTiptapTextAlignLeftExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [
		TextAlign.configure({
			types: ['heading', 'paragraph', 'blockquote', 'orderedList', 'bulletList', 'codeBlock'],
		}),
	];

	override isActive(editor?: Editor) {
		return editor?.isActive({ textAlign: 'left' }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().setTextAlign('left').run();
	}
}
