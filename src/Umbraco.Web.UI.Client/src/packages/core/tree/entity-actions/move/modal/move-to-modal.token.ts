import type { UmbTreeItemModel } from '../../../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_MOVE_TO_MODAL_ALIAS = 'Umb.Modal.MoveTo';

export interface UmbMoveToModalSelectionResult {
	valid: boolean;
	error?: string;
}

export interface UmbMoveToModalSubmitResult {
	success: boolean;
	error?: { message: string };
}

export interface UmbMoveToModalData extends UmbEntityModel {
	treeAlias: string;
	foldersOnly?: boolean;
	pickableFilter?: (item: UmbTreeItemModel) => boolean;
	onSelection?: (destinationUnique: string | null) => Promise<UmbMoveToModalSelectionResult>;
	onBeforeSubmit?: (destinationUnique: string | null) => Promise<UmbMoveToModalSubmitResult>;
}

export interface UmbMoveToModalValue {
	destination: {
		unique: string | null;
	};
}

export const UMB_MOVE_TO_MODAL = new UmbModalToken<UmbMoveToModalData, UmbMoveToModalValue>(UMB_MOVE_TO_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
