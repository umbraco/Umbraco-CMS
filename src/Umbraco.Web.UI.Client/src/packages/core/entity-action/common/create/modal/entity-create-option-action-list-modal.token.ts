import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityCreateOptionActionListModalData {
	unique: UmbEntityUnique;
	entityType: string;
}

export type UmbEntityCreateOptionActionListModalValue = never;

export const UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL = new UmbModalToken<
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue
>('Umb.Modal.Entity.CreateOptionActionList', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
