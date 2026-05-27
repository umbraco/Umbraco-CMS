export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.WordCount',
		name: 'Word Count Tiptap Extension',
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapWordCountExtensionApi })),
		meta: {
			icon: 'icon-speed-gauge',
			label: 'Word Count',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapStatusbarExtension',
		alias: 'Umb.Tiptap.Statusbar.WordCount',
		name: 'Word Count Tiptap Statusbar Extension',
		element: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapStatusbarWordCountElement })),
		forExtensions: ['Umb.Tiptap.WordCount'],
		meta: {
			alias: 'wordCount',
			icon: 'icon-speed-gauge',
			label: 'Word Count',
		},
	},
];
