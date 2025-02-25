import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbBulkDeleteWithRelationConfirmModalData {
	uniques: Array<string>;
	itemRepositoryAlias: string;
	referenceRepositoryAlias: string;
}

export type UmbBulkDeleteWithRelationConfirmModalValue = undefined;

export const UMB_BULK_DELETE_WITH_RELATION_CONFIRM_MODAL = new UmbModalToken<
	UmbBulkDeleteWithRelationConfirmModalData,
	UmbBulkDeleteWithRelationConfirmModalValue
>('Umb.Modal.BulkDeleteWithRelation', {
	modal: {
		type: 'dialog',
	},
});
