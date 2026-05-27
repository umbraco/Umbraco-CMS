import UmbTiptapToolbarTextColorBackgroundExtensionApi from './text-color-background.tiptap-toolbar-api.js';
import UmbTiptapToolbarTextColorForegroundExtensionApi from './text-color-foreground.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'colorPickerButton',
		alias: 'Umb.Tiptap.Toolbar.TextColorBackground',
		name: 'Text Color Background Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextColorBackgroundExtensionApi,
		forExtensions: ['Umb.Tiptap.HtmlAttributeStyle', 'Umb.Tiptap.HtmlTagSpan'],
		meta: {
			alias: 'text-color-background',
			icon: 'icon-color-bucket',
			label: 'Background color',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'colorPickerButton',
		alias: 'Umb.Tiptap.Toolbar.TextColorForeground',
		name: 'Text Color Foreground Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextColorForegroundExtensionApi,
		forExtensions: ['Umb.Tiptap.HtmlAttributeStyle', 'Umb.Tiptap.HtmlTagSpan'],
		meta: {
			alias: 'text-color-foreground',
			icon: 'icon-colorpicker',
			label: 'Color',
		},
	},
];
