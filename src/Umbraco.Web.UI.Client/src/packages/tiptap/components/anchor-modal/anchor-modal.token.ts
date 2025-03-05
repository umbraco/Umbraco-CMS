import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbAnchorModalData = {
	id?: string;
};

export type UmbAnchorModalValue = string;

export const UMB_ANCHOR_MODAL = new UmbModalToken<UmbAnchorModalData, UmbAnchorModalValue>('Umb.Modal.Anchor', {
	modal: { size: 'medium', type: 'dialog' },
});
