export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embedded Media Tiptap Extension',
		api: () => import('./embedded-media.tiptap-api.js'),
		meta: {
			icon: 'icon-embed',
			label: '#general_embed',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.EmbeddedMedia',
		name: 'Embedded Media Tiptap Toolbar Extension',
		api: () => import('./embedded-media.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Embed'],
		meta: {
			alias: 'umbEmbeddedMedia',
			icon: 'icon-embed',
			label: '#general_embed',
		},
	},
];
