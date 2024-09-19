import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	alias: 'Umb.Tiptap.Image',
	name: 'Image Tiptap Extension',
	api: () => import('./image.extension.js'),
	meta: {
		alias: 'image',
	},
};

export default class UmbTiptapImageExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions() {
		return [UmbImage.configure({ inline: true })];
	}
}
