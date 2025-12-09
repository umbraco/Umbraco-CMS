import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityDeleteModalData {
	unique: UmbEntityUnique;
	entityType: string;
	itemRepositoryAlias: string;
	headline?: string;
	message?: string;
}

export type UmbEntityDeleteModalValue = never;

export const UMB_ENTITY_DELETE_MODAL = new UmbModalToken<UmbEntityDeleteModalData, UmbEntityDeleteModalValue>(
	'Umb.Modal.Entity.Delete',
	{
		modal: {
			type: 'dialog',
		},
	},
);
