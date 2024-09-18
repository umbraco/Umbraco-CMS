import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { BulletList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.BulletList',
	name: 'Bullet List Tiptap Extension',
	api: () => import('./bullet-list.extension.js'),
	weight: 993,
	meta: {
		alias: 'bullet-list',
		icon: 'bullet-list',
		label: 'Bullet List',
	},
};

export default class UmbTiptapBulletListExtensionApi extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [BulletList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBulletList().run();
	}
}
