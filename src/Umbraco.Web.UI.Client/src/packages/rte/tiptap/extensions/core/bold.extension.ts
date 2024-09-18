import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Bold } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Bold',
	name: 'Bold Tiptap Extension',
	api: () => import('./bold.extension.js'),
	weight: 999,
	meta: {
		alias: 'bold',
		icon: 'bold',
		label: 'Bold',
	},
};

export default class UmbTiptapBoldPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Bold];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBold().run();
	}
}
