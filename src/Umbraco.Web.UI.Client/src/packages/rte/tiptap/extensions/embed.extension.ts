import { UmbTiptapExtensionApi } from './types.js';
import type { ManifestTiptapExtension } from './tiptap-extension.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Embed',
	name: 'Embed Tiptap Extension',
	api: () => import('./embed.extension.js'),
	meta: {
		alias: 'umb-embed',
		icon: 'umbraco',
		label: 'Embed',
	},
};

export default class UmbTiptapEmbedPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [];

	override async execute(editor?: Editor) {
		console.log('umb-embed.execute', editor);
		// Research: https://github.com/ueberdosis/tiptap/tree/main/packages/extension-youtube
	}
}
