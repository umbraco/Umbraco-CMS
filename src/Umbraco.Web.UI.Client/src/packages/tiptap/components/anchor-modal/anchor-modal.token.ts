import { UMB_TIPTAP_ANCHOR_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbTiptapAnchorModalData = {
	id?: string;
};

export type UmbTiptapAnchorModalValue = string;

export const UMB_TIPTAP_ANCHOR_MODAL = new UmbModalToken<UmbTiptapAnchorModalData, UmbTiptapAnchorModalValue>(
	UMB_TIPTAP_ANCHOR_MODAL_ALIAS,
	{
		modal: { type: 'dialog' },
	},
);
