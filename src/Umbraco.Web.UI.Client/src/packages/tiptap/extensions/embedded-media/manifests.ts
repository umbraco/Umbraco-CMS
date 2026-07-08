export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embedded Media Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapEmbeddedMediaExtensionApi })),
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
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarEmbeddedMediaExtensionApi })),
		forExtensions: ['Umb.Tiptap.Embed'],
		meta: {
			alias: 'umbEmbeddedMedia',
			icon: 'icon-embed',
			label: '#general_embed',
		},
	},
];
