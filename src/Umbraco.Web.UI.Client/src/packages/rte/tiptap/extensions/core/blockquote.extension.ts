import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Blockquote } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Blockquote',
	name: 'Blockquote Tiptap Extension',
	api: () => import('./blockquote.extension.js'),
	weight: 995,
	meta: {
		alias: 'blockquote',
		icon: 'blockquote',
		label: 'Blockquote',
	},
};

export default class UmbTiptapBlockquoteExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Blockquote];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBlockquote().run();
	}
}
