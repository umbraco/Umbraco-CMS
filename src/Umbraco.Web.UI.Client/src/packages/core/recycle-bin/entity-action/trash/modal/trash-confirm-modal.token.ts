import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTrashConfirmModalData {
	unique: string;
	entityType: string;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
}

export type UmbTrashConfirmModalValue = undefined;

export const UMB_TRASH_CONFIRM_MODAL = new UmbModalToken<UmbTrashConfirmModalData, UmbTrashConfirmModalValue>(
	'Umb.Modal.Trash.Confirm',
	{
		modal: {
			type: 'dialog',
		},
	},
);
