import { UMB_DUPLICATE_TO_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDuplicateToModalData {
	unique: string | null;
	entityType: string;
	treeAlias: string;
}

export interface UmbDuplicateToModalValue {
	destination: {
		unique: string | null;
	};
}

export const UMB_DUPLICATE_TO_MODAL = new UmbModalToken<UmbDuplicateToModalData, UmbDuplicateToModalValue>(
	UMB_DUPLICATE_TO_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
