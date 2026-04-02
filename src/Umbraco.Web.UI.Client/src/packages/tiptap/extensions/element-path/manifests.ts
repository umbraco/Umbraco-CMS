export const manifests: Array<UmbExtensionManifest> = [
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
