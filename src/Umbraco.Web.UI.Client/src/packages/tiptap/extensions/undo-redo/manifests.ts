import UmbTiptapToolbarRedoExtensionApi from './redo.tiptap-toolbar-api.js';
import UmbTiptapToolbarUndoExtensionApi from './undo.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'actionButton',
		alias: 'Umb.Tiptap.Toolbar.Undo',
		name: 'Undo Tiptap Toolbar Extension',
		api: UmbTiptapToolbarUndoExtensionApi,
		meta: {
			alias: 'undo',
			icon: 'icon-undo',
			label: '#buttons_undo',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'actionButton',
		alias: 'Umb.Tiptap.Toolbar.Redo',
		name: 'Redo Tiptap Toolbar Extension',
		api: UmbTiptapToolbarRedoExtensionApi,
		meta: {
			alias: 'redo',
			icon: 'icon-redo',
			label: '#buttons_redo',
		},
	},
];
