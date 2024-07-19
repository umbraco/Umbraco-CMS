export const manifests = [
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.BlockPicker',
		name: 'Block Picker TinyMCE Plugin',
		js: () => import('./tiny-mce-block-picker.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'umbblockpicker',
					label: '#blockEditor_insertBlock',
					icon: 'visualblocks',
				},
			],
		},
	},
];
