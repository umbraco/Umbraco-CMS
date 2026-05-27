import UmbTiptapLinkExtensionApi from './link.tiptap-api.js';
import UmbTiptapToolbarLinkExtensionApi from './link.tiptap-toolbar-api.js';
import UmbTiptapToolbarUnlinkExtensionApi from './unlink.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Link',
		name: 'Link Tiptap Extension',
		api: UmbTiptapLinkExtensionApi,
		meta: {
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Link',
		name: 'Link Tiptap Toolbar Extension',
		api: UmbTiptapToolbarLinkExtensionApi,
		forExtensions: ['Umb.Tiptap.Link'],
		meta: {
			alias: 'umbLink',
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'actionButton',
		alias: 'Umb.Tiptap.Toolbar.Unlink',
		name: 'Unlink Tiptap Toolbar Extension',
		api: UmbTiptapToolbarUnlinkExtensionApi,
		forExtensions: ['Umb.Tiptap.Link'],
		meta: {
			alias: 'unlink',
			icon: 'icon-unlink',
			label: 'Unlink',
		},
	},
];
