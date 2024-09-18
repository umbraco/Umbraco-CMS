import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Strike } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Strike',
	name: 'Strike Tiptap Extension',
	api: () => import('./strike.extension.js'),
	weight: 996,
	meta: {
		alias: 'strike',
		icon: 'strike',
		label: 'Strike',
	},
};

export default class UmbTiptapStrikePlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Strike];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleStrike().run();
	}
}
