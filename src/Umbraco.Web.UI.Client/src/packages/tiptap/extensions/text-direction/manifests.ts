import UmbTiptapTextDirectionExtensionApi from './text-direction.tiptap-api.js';
import UmbTiptapToolbarTextDirectionLtrExtensionApi from './text-direction-ltr.tiptap-toolbar-api.js';
import UmbTiptapToolbarTextDirectionRtlExtensionApi from './text-direction-rtl.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TextDirection',
		name: 'Text Direction Tiptap Extension',
		api: UmbTiptapTextDirectionExtensionApi,
		meta: {
			icon: 'icon-text-direction-ltr',
			label: 'Text Direction',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextDirectionRtl',
		name: 'Text Direction RTL Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextDirectionRtlExtensionApi,
		forExtensions: ['Umb.Tiptap.TextDirection'],
		meta: {
			alias: 'text-direction-rtl',
			icon: 'icon-text-direction-rtl',
			label: 'Right to left',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.TextDirectionLtr',
		name: 'Text Direction LTR Tiptap Toolbar Extension',
		api: UmbTiptapToolbarTextDirectionLtrExtensionApi,
		forExtensions: ['Umb.Tiptap.TextDirection'],
		meta: {
			alias: 'text-direction-ltr',
			icon: 'icon-text-direction-ltr',
			label: 'Left to right',
		},
	},
];
