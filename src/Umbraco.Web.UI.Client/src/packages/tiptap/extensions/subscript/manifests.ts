import UmbTiptapSubscriptExtensionApi from './subscript.tiptap-api.js';
import UmbTiptapToolbarSubscriptExtensionApi from './subscript.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Subscript',
		name: 'Subscript Tiptap Extension',
		api: UmbTiptapSubscriptExtensionApi,
		meta: {
			icon: 'icon-subscript',
			label: 'Subscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Subscript',
		name: 'Subscript Tiptap Toolbar Extension',
		api: UmbTiptapToolbarSubscriptExtensionApi,
		forExtensions: ['Umb.Tiptap.Subscript'],
		meta: {
			alias: 'subscript',
			icon: 'icon-subscript',
			label: 'Subscript',
		},
	},
];
