import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	alias: 'Umb.Tiptap.Image',
	name: 'Image Tiptap Extension',
	api: () => import('./image.extension.js'),
	meta: {},
};

export default class UmbTiptapImageExtensionApi extends UmbTiptapExtensionApi {
	getTiptapExtensions() {
		return [UmbImage.configure({ inline: true })];
	}
}
