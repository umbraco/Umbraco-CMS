export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.RichTextEssentials',
		name: 'Rich Text Essentials Tiptap Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapRichTextEssentialsExtensionApi })),
		weight: 1000,
		meta: {
			icon: 'icon-browser-window',
			label: 'Rich Text Essentials',
			group: '#tiptap_extGroup_formatting',
			description: 'This is a core extension, it is always enabled by default.',
		},
	},
];
