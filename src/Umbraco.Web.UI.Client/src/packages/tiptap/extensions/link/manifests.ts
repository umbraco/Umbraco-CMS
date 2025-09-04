export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Link',
		name: 'Link Tiptap Extension',
		api: () => import('./link.tiptap-api.js'),
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
		api: () => import('./link.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Link'],
		meta: {
			alias: 'umbLink',
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Unlink',
		name: 'Unlink Tiptap Toolbar Extension',
		api: () => import('./unlink.tiptap-toolbar-api.js'),
		element: () => import('../../components/toolbar/tiptap-toolbar-button-disabled.element.js'),
		forExtensions: ['Umb.Tiptap.Link'],
		meta: {
			alias: 'unlink',
			icon: 'icon-unlink',
			label: 'Unlink',
		},
	},
];
