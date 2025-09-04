export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.MediaPicker',
		name: 'Media Picker Tiptap Toolbar Extension',
		api: () => import('./media-picker.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Figure', 'Umb.Tiptap.Image'],
		meta: {
			alias: 'umbMedia',
			icon: 'icon-picture',
			label: '#general_mediaPicker',
		},
	},
];
