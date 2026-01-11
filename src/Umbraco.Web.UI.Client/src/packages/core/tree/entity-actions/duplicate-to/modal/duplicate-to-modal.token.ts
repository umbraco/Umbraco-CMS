import type { UmbTreeItemModel } from '../../../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DUPLICATE_TO_MODAL_ALIAS = 'Umb.Modal.DuplicateTo';

export interface UmbDuplicateToModalSelectionResult {
	valid: boolean;
	error?: string;
}

export interface UmbDuplicateToModalSubmitResult {
	success: boolean;
	error?: { message: string };
}

export interface UmbDuplicateToModalData extends UmbEntityModel {
	treeAlias: string;
	name?: string;
	foldersOnly?: boolean;
	pickableFilter?: (item: UmbTreeItemModel) => boolean;
	onSelection?: (destinationUnique: string | null) => Promise<UmbDuplicateToModalSelectionResult>;
	onBeforeSubmit?: (destinationUnique: string | null) => Promise<UmbDuplicateToModalSubmitResult>;
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
