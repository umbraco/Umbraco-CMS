import { UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.CharacterMap',
		name: 'Character Map Tiptap Toolbar Extension',
		api: () => import('./character-map.tiptap-toolbar-api.js'),
		meta: {
			alias: 'umbCharacterMap',
			icon: 'icon-omega',
			label: '#tiptap_charmap',
		},
	},
	{
		type: 'modal',
		alias: UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS,
		name: 'Character Map Modal',
		element: () => import('./modals/character-map-modal.element.js'),
	},
];
