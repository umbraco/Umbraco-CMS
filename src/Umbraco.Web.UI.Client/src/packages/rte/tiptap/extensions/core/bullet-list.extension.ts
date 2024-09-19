import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { BulletList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.BulletList',
	name: 'Bullet List Tiptap Extension',
	api: () => import('./bullet-list.extension.js'),
	weight: 993,
	meta: {
		alias: 'bulletList',
		icon: 'bullet-list',
		label: 'Bullet List',
	},
};

export default class UmbTiptapBulletListExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [BulletList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBulletList().run();
	}
}
