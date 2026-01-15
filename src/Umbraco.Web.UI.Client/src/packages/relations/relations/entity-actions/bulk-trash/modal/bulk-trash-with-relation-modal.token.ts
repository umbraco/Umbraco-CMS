import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBulkTrashWithRelationConfirmModalData {
	uniques: Array<string>;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
}

export type UmbBulkTrashWithRelationConfirmModalValue = undefined;

export const UMB_BULK_TRASH_WITH_RELATION_CONFIRM_MODAL = new UmbModalToken<
	UmbBulkTrashWithRelationConfirmModalData,
	UmbBulkTrashWithRelationConfirmModalValue
>('Umb.Modal.BulkTrashWithRelation', {
	modal: {
		type: 'dialog',
	},
});
