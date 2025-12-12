import type { UmbEntityDeleteModalData } from '@umbraco-cms/backoffice/entity-action';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDeleteWithRelationConfirmModalData extends UmbEntityDeleteModalData {
	referenceRepositoryAlias: string;
}

export type UmbDeleteWithRelationConfirmModalValue = never;

export const UMB_DELETE_WITH_RELATION_CONFIRM_MODAL = new UmbModalToken<
	UmbDeleteWithRelationConfirmModalData,
	UmbDeleteWithRelationConfirmModalValue
>('Umb.Modal.DeleteWithRelation', {
	modal: {
		type: 'dialog',
	},
});
