import { UmbCharacterMapModalElement } from './modals/character-map-modal.element.js';
import UmbTiptapToolbarCharacterMapExtensionApi from './character-map.tiptap-toolbar-api.js';
import { UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.CharacterMap',
		name: 'Character Map Tiptap Toolbar Extension',
		api: UmbTiptapToolbarCharacterMapExtensionApi,
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
		element: UmbCharacterMapModalElement,
	},
];
