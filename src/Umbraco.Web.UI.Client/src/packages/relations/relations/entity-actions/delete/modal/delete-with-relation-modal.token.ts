import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDeleteWithRelationConfirmModalData {
	unique: string;
	entityType: string;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
}

export type UmbDeleteWithRelationConfirmModalValue = undefined;

export const UMB_DELETE_WITH_RELATION_CONFIRM_MODAL = new UmbModalToken<
	UmbDeleteWithRelationConfirmModalData,
	UmbDeleteWithRelationConfirmModalValue
>('Umb.Modal.DeleteWithRelation', {
	modal: {
		type: 'dialog',
	},
});
