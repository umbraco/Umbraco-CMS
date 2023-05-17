import type { ManifestTinyMcePlugin } from '@umbraco-cms/backoffice/extension-registry';

const pluginBaseUrl = '/src/backoffice/core/property-editors/uis/tiny-mce/plugins/';
const pluginManifests: Array<ManifestTinyMcePlugin> = [
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.CodeEditor',
		name: 'Code Editor TinyMCE Plugin',
		js: `${pluginBaseUrl}tiny-mce-code-editor.plugin`,
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.LinkPicker',
		name: 'Link Picker TinyMCE Plugin',
		js: `${pluginBaseUrl}tiny-mce-linkpicker.plugin`,
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MediaPicker',
		name: 'Media Picker TinyMCE Plugin',
		js: `${pluginBaseUrl}tiny-mce-mediapicker.plugin`,
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.EmbeddedMedia',
		name: 'Embedded Media TinyMCE Plugin',
		js: `${pluginBaseUrl}tiny-mce-embeddedmedia.plugin`,
	},
	{
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MacroPicker',
		name: 'Macro Picker TinyMCE Plugin',
		js: `${pluginBaseUrl}tiny-mce-macropicker.plugin`,
	},
];

export const manifests = [...pluginManifests];
