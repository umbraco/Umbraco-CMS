import UmbTiptapToolbarClearFormattingExtensionApi from './clear-formatting.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.ClearFormatting',
		name: 'Clear Formatting Tiptap Toolbar Extension',
		api: UmbTiptapToolbarClearFormattingExtensionApi,
		meta: {
			alias: 'clear-formatting',
			icon: 'icon-clear-formatting',
			label: 'Clear Formatting',
		},
	},
];
