export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.ClearFormatting',
		name: 'Clear Formatting Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarClearFormattingExtensionApi })),
		meta: {
			alias: 'clear-formatting',
			icon: 'icon-clear-formatting',
			label: 'Clear Formatting',
		},
	},
];
