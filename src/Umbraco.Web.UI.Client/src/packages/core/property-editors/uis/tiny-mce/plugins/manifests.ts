import type { ManifestTinyMcePlugin } from '@umbraco-cms/backoffice/extension-registry';

const pluginManifests: Array<ManifestTinyMcePlugin> = [
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.CodeEditor',
		name: 'Code Editor TinyMCE Plugin',
		loader: () => import('./tiny-mce-code-editor.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'sourcecode',
					label: 'Source code editor',
					icon: 'umb:code',
				},
			],
		},
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.LinkPicker',
		name: 'Link Picker TinyMCE Plugin',
		loader: () => import('./tiny-mce-linkpicker.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'link',
					label: 'Insert/Edit link',
					icon: 'umb:link',
				},
				{
					alias: 'unlink',
					label: 'Remove link',
					icon: 'umb:link',
				},
			],
		},
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MediaPicker',
		name: 'Media Picker TinyMCE Plugin',
		loader: () => import('./tiny-mce-mediapicker.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'umbmediapicker',
					label: 'Media picker',
					icon: 'umb:image',
				},
			],
		},
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.EmbeddedMedia',
		name: 'Embedded Media TinyMCE Plugin',
		loader: () => import('./tiny-mce-embeddedmedia.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'umbembeddialog',
					label: 'Embed',
					icon: 'umb:embed',
				},
			],
		},
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MacroPicker',
		name: 'Macro Picker TinyMCE Plugin',
		loader: () => import('./tiny-mce-macropicker.plugin.js'),
		meta: {
			toolbar: [
				{
					alias: 'umbmacro',
					label: 'Macro',
					icon: 'umb:preferences',
				},
			],
		},
	},
];

export const manifests = [...pluginManifests];
