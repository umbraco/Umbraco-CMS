import { UMB_DUPLICATE_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDuplicateModalData {
	unique: string | null;
	entityType: string;
	treeAlias: string;
}

export interface UmbDuplicateModalValue {
	destination: {
		unique: string | null;
	};
	relateToOriginal: boolean;
	includeDescendants: boolean;
}

export const UMB_DUPLICATE_MODAL = new UmbModalToken<UmbDuplicateModalData, UmbDuplicateModalValue>(
	UMB_DUPLICATE_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
