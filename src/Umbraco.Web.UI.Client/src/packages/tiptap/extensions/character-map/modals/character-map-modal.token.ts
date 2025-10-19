import { UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbTiptapCharacterMapModalData = never;

export type UmbTiptapCharacterMapModalValue = string;

export const UMB_TIPTAP_CHARACTER_MAP_MODAL = new UmbModalToken<
	UmbTiptapCharacterMapModalData,
	UmbTiptapCharacterMapModalValue
>(UMB_TIPTAP_CHARACTER_MAP_MODAL_ALIAS, {
	modal: { type: 'dialog' },
});
