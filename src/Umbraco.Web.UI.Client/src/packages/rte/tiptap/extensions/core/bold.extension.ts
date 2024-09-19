import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Bold } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Bold',
	name: 'Bold Tiptap Extension',
	api: () => import('./bold.extension.js'),
	weight: 999,
	meta: {
		alias: 'bold',
		icon: 'icon-bold',
		label: 'Bold',
	},
};

export default class UmbTiptapBoldExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Bold];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBold().run();
	}
}
