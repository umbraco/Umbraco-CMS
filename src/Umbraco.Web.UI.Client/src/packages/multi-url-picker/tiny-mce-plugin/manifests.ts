export const manifests = [
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MultiUrlPicker',
		name: 'Multi Url Picker TinyMCE Plugin',
		js: () => import('./tiny-mce-multi-url-picker.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'link',
					label: 'Insert/Edit link',
					icon: 'link',
				},
				{
					alias: 'unlink',
					label: 'Remove link',
					icon: 'unlink',
				},
			],
		},
	},
];
