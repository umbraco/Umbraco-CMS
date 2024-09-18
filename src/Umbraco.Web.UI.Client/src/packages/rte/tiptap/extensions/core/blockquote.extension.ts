import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Blockquote } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
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

export default class UmbTiptapBlockquotePlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Blockquote];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBlockquote().run();
	}
}
