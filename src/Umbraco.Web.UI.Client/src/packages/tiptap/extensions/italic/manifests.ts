import UmbTiptapItalicExtensionApi from './italic.tiptap-api.js';
import UmbTiptapToolbarItalicExtensionApi from './italic.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Italic',
		name: 'Italic Tiptap Extension',
		api: UmbTiptapItalicExtensionApi,
		meta: {
			icon: 'icon-italic',
			label: 'Italic',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Italic',
		name: 'Italic Tiptap Toolbar Extension',
		api: UmbTiptapToolbarItalicExtensionApi,
		forExtensions: ['Umb.Tiptap.Italic'],
		meta: {
			alias: 'italic',
			icon: 'icon-italic',
			label: '#buttons_italic',
		},
	},
];
