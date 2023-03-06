import { TinyMceCodeEditorPlugin } from './tiny-mce-code-editor.plugin';
import { TinyMceEmbeddedMediaPlugin } from './tiny-mce-embeddedmedia.plugin';
import { TinyMceLinkPickerPlugin } from './tiny-mce-linkpicker.plugin';
import { TinyMceMacroPickerPlugin } from './tiny-mce-macropicker.plugin';
import { TinyMceMediaPickerPlugin } from './tiny-mce-mediapicker.plugin';
import { ManifestTinyMcePlugin } from 'libs/extensions-registry/tinymce-plugin.model';

const pluginManifests: Array<ManifestTinyMcePlugin> = [
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.CodeEditor',
		name: 'Code Editor TinyMCE Plugin',
		meta: {
			api: TinyMceCodeEditorPlugin,
		},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.LinkPicker',
		name: 'Link Picker TinyMCE Plugin',
		meta: {
			api: TinyMceLinkPickerPlugin,
		},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MediaPicker',
		name: 'Media Picker TinyMCE Plugin',
		meta: {
			api: TinyMceMediaPickerPlugin,
		},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.EmbeddedMedia',
		name: 'Embedded Media TinyMCE Plugin',
		meta: {
			api: TinyMceEmbeddedMediaPlugin,
		},
	},
    {
		type: 'tinyMcePlugin',
		alias: 'Umb.TinyMcePlugin.MacroPicker',
		name: 'Macro Picker TinyMCE Plugin',
		meta: {
			api: TinyMceMacroPickerPlugin,
		},
	},
];

export const manifests = [...pluginManifests];	
