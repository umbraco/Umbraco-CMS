import { UMB_TIPTAP_ANCHOR_MODAL_ALIAS } from './modals/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Anchor',
		name: 'Anchor Tiptap Extension',
		api: () => import('./anchor.tiptap-api.js'),
		meta: {
			icon: 'icon-anchor',
			label: '#tiptap_anchor',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Anchor',
		name: 'Anchor Tiptap Toolbar Extension',
		api: () => import('./anchor.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Anchor'],
		meta: {
			alias: 'anchor',
			icon: 'icon-anchor',
			label: '#tiptap_anchor',
		},
	},
	{
		type: 'modal',
		alias: UMB_TIPTAP_ANCHOR_MODAL_ALIAS,
		name: 'Tiptap Anchor Modal',
		element: () => import('./modals/anchor-modal.element.js'),
	},
];
