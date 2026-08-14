export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.MediaPicker',
		name: 'Media Picker Tiptap Toolbar Extension',
		api: () =>
			import('../extension-apis.bundle.js').then((m) => ({
				default: m.UmbTiptapToolbarMediaPickerToolbarExtensionApi,
			})),
		forExtensions: ['Umb.Tiptap.Figure', 'Umb.Tiptap.Image'],
		meta: {
			alias: 'umbMedia',
			icon: 'icon-picture',
			label: '#general_mediaPicker',
		},
	},
];
