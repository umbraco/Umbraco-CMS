import type { ManifestTinyMcePlugin } from '@umbraco-cms/backoffice/extension-registry';

const pluginManifests: Array<ManifestTinyMcePlugin> = [
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.CodeEditor',
		name: 'Code Editor TinyMCE Plugin',
		loader: () => import('./tiny-mce-code-editor.plugin.js'),
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.LinkPicker',
		name: 'Link Picker TinyMCE Plugin',
		loader: () => import('./tiny-mce-linkpicker.plugin.js'),
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MediaPicker',
		name: 'Media Picker TinyMCE Plugin',
		loader: () => import('./tiny-mce-mediapicker.plugin.js'),
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.EmbeddedMedia',
		name: 'Embedded Media TinyMCE Plugin',
		loader: () => import('./tiny-mce-embeddedmedia.plugin.js'),
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MacroPicker',
		name: 'Macro Picker TinyMCE Plugin',
		loader: () => import('./tiny-mce-macropicker.plugin.js'),
	},
];

export const manifests = [...pluginManifests];
