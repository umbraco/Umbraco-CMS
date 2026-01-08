import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTrashWithRelationConfirmModalData {
	unique: string;
	entityType: string;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
}

export type UmbTrashWithRelationConfirmModalValue = undefined;

export const UMB_TRASH_WITH_RELATION_CONFIRM_MODAL = new UmbModalToken<
	UmbTrashWithRelationConfirmModalData,
	UmbTrashWithRelationConfirmModalValue
>('Umb.Modal.TrashWithRelation', {
	modal: {
		type: 'dialog',
	},
});
