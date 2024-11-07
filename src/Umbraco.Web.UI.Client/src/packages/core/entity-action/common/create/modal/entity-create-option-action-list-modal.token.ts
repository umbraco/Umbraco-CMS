import { UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL_ALIAS } from './constants.js';
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
>(UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
