import type { ManifestTinyMcePlugin } from '@umbraco-cms/backoffice/extension-registry';

const pluginManifests: Array<ManifestTinyMcePlugin> = [
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
		alias: 'Umb.TinyMcePlugin.LinkPicker',
		name: 'Link Picker TinyMCE Plugin',
		js: () => import('./tiny-mce-linkpicker.plugin.js'),
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
];

export const manifests = [...pluginManifests];
