export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapStatusbarExtension',
		alias: 'Umb.Tiptap.Statusbar.WordCount',
		name: 'Word Count Tiptap Statusbar Extension',
		element: () => import('./word-count.tiptap-statusbar-element.js'),
		forExtensions: ['Umb.Tiptap.WordCount'],
		meta: {
			alias: 'wordCount',
			icon: 'icon-speed-gauge',
			label: 'Word Count',
		},
	},
];
