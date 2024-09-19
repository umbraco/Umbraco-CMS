import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { OrderedList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.OrderedList',
	name: 'Ordered List Tiptap Extension',
	api: () => import('./ordered-list.extension.js'),
	weight: 992,
	meta: {
		alias: 'orderedList',
		icon: 'ordered-list',
		label: 'Ordered List',
	},
};

export default class UmbTiptapOrderedListExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [OrderedList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleOrderedList().run();
	}
}
