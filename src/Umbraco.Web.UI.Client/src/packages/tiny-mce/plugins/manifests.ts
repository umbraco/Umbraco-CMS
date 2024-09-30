import type { ManifestTinyMcePlugin } from './tinymce-plugin.extension.js';

export const manifests: Array<ManifestTinyMcePlugin> = [
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.CodeEditor',
		name: 'Code Editor TinyMCE Plugin',
		js: () => import('./tiny-mce-code-editor.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'sourcecode',
					label: 'Source code editor',
					icon: 'sourcecode',
				},
			],
		},
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MediaPicker',
		name: 'Media Picker TinyMCE Plugin',
		js: () => import('./tiny-mce-mediapicker.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'umbmediapicker',
					label: 'Image',
					icon: 'image',
				},
			],
		},
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.EmbeddedMedia',
		name: 'Embedded Media TinyMCE Plugin',
		js: () => import('./tiny-mce-embeddedmedia.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'umbembeddialog',
					label: 'Embed',
					icon: 'embed',
				},
			],
		},
	},
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
