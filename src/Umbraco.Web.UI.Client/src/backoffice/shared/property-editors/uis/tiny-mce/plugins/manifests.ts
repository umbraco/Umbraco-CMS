import { ManifestTinyMcePlugin } from 'libs/extensions-registry/tinymce-plugin.model';

const pluginManifests: Array<ManifestTinyMcePlugin> = [
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.CodeEditor',
		name: 'Code Editor TinyMCE Plugin',
		meta: {
			exportName: 'TinyMceCodeEditorPlugin',
			js: '/src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-code-editor.plugin', 
		},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.LinkPicker',
		name: 'Link Picker TinyMCE Plugin',
		meta: {
			exportName: 'TinyMceLinkPickerPlugin',
			js: '/src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-linkpicker.plugin',
		},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MediaPicker',
		name: 'Media Picker TinyMCE Plugin',
		meta: {
			exportName: 'TinyMceMediaPickerPlugin',
			js: '/src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-mediapicker.plugin', },
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.EmbeddedMedia',
		name: 'Embedded Media TinyMCE Plugin',
		meta: {
			exportName: 'TinyMceEmbeddedMediaPlugin',
			js: '/src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-embeddedmedia.plugin', 	},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MacroPicker',
		name: 'Macro Picker TinyMCE Plugin',
		meta: {
			exportName: 'TinyMceMacroPickerPlugin',
			js: '/src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-macropicker.plugin', },
	},
];

export const manifests = [...pluginManifests];	
