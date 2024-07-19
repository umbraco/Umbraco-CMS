import { UMB_DUPLICATE_TO_MODAL_ALIAS } from './constants.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDuplicateToModalData extends UmbEntityModel {
	treeAlias: string;
	foldersOnly?: boolean;
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
