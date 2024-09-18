import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { OrderedList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.OrderedList',
	name: 'Ordered List Tiptap Extension',
	api: () => import('./ordered-list.extension.js'),
	weight: 992,
	meta: {
		alias: 'ordered-list',
		icon: 'ordered-list',
		label: 'Ordered List',
	},
};

export default class UmbTiptapOrderedListExtensionApi extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [OrderedList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleOrderedList().run();
	}
}
