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
	{
		type: 'tiptapStatusbarExtension',
		alias: 'Umb.Tiptap.Statusbar.ElementPath',
		name: 'Element Path Tiptap Statusbar Extension',
		element: () => import('./element-path.tiptap-statusbar-element.js'),
		meta: {
			alias: 'elementPath',
			icon: 'icon-map-alt',
			label: 'Element Path',
		},
	},
];
