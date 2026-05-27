import UmbTiptapToolbarSourceEditorExtensionApi from './source-editor.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.SourceEditor',
		name: 'Source Editor Tiptap Toolbar Extension',
		api: UmbTiptapToolbarSourceEditorExtensionApi,
		meta: {
			alias: 'umbSourceEditor',
			icon: 'icon-code-xml',
			label: '#general_viewSourceCode',
		},
	},
];
