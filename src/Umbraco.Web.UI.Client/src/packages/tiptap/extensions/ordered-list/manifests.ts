import UmbTiptapOrderedListExtensionApi from './ordered-list.tiptap-api.js';
import UmbTiptapToolbarOrderedListExtensionApi from './ordered-list.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.OrderedList',
		name: 'Ordered List Tiptap Extension',
		api: UmbTiptapOrderedListExtensionApi,
		meta: {
			icon: 'icon-ordered-list',
			label: 'Ordered List',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.OrderedList',
		name: 'Ordered List Tiptap Toolbar Extension',
		api: UmbTiptapToolbarOrderedListExtensionApi,
		forExtensions: ['Umb.Tiptap.OrderedList'],
		meta: {
			alias: 'orderedList',
			icon: 'icon-ordered-list',
			label: 'Ordered List',
		},
	},
];
