import UmbTiptapTextAlignExtensionApi from './text-align.tiptap-api.js';
import UmbTiptapToolbarTextAlignCenterExtensionApi from './text-align-center.tiptap-toolbar-api.js';
import UmbTiptapToolbarTextAlignJustifyExtensionApi from './text-align-justify.tiptap-toolbar-api.js';
import UmbTiptapToolbarTextAlignLeftExtensionApi from './text-align-left.tiptap-toolbar-api.js';
import UmbTiptapToolbarTextAlignRightExtensionApi from './text-align-right.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TextAlign',
		name: 'Text Align Tiptap Extension',
		api: UmbTiptapTextAlignExtensionApi,
		meta: {
			icon: 'icon-text-align-justify',
			label: 'Text Align',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignLeft',
		name: 'Text Align Left Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextAlignLeftExtensionApi,
		forExtensions: ['Umb.Tiptap.TextAlign'],
		meta: {
			alias: 'text-align-left',
			icon: 'icon-text-align-left',
			label: 'Text Align Left',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignCenter',
		name: 'Text Align Center Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextAlignCenterExtensionApi,
		forExtensions: ['Umb.Tiptap.TextAlign'],
		meta: {
			alias: 'text-align-center',
			icon: 'icon-text-align-center',
			label: 'Text Align Center',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignRight',
		name: 'Text Align Right Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextAlignRightExtensionApi,
		forExtensions: ['Umb.Tiptap.TextAlign'],
		meta: {
			alias: 'text-align-right',
			icon: 'icon-text-align-right',
			label: 'Text Align Right',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextAlignJustify',
		name: 'Text Align Justify Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextAlignJustifyExtensionApi,
		forExtensions: ['Umb.Tiptap.TextAlign'],
		meta: {
			alias: 'text-align-justify',
			icon: 'icon-text-align-justify',
			label: 'Text Align Justify',
		},
	},
];
