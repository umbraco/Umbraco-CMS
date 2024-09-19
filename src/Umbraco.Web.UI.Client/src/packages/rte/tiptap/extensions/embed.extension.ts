import { UmbTiptapToolbarElementApiBase } from './types.js';
import type { ManifestTiptapExtensionButtonKind } from './tiptap-extension.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
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

export default class UmbTiptapEmbedExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [];

	override async execute(editor?: Editor) {
		console.log('umb-embed.execute', editor);
		// Research: https://github.com/ueberdosis/tiptap/tree/main/packages/extension-youtube
	}
}
