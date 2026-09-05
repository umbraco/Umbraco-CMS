export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapStatusbarExtension',
		alias: 'Umb.Tiptap.Statusbar.ElementPath',
		name: 'Element Path Tiptap Statusbar Extension',
		element: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapStatusbarElementPathElement })),
		meta: {
			alias: 'elementPath',
			icon: 'icon-map-alt',
			label: 'Element Path',
		},
	},
];
