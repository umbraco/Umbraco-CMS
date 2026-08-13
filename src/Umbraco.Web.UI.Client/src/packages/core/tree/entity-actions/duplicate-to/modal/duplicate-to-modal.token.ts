import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';

export const UMB_DUPLICATE_TO_MODAL_ALIAS = 'Umb.Modal.DuplicateTo';

/**
 * @deprecated Deprecated since v17. The "Duplicate to" entity action now uses the shared `UMB_TREE_PICKER_MODAL`. Scheduled for removal in Umbraco 19.
 */
export interface UmbDuplicateToModalData extends UmbEntityModel {
	treeAlias: string;
	foldersOnly?: boolean;
	treeExpansion?: UmbEntityExpansionModel;
}

/**
 * @deprecated Deprecated since v17. The "Duplicate to" entity action now uses the shared `UMB_TREE_PICKER_MODAL`. Scheduled for removal in Umbraco 19.
 */
export interface UmbDuplicateToModalValue {
	destination: {
		unique: string | null;
	};
}

/**
 * @deprecated Deprecated since v17. Use the shared `UMB_TREE_PICKER_MODAL` instead, passing `headline` and `confirmLabel`. Scheduled for removal in Umbraco 19.
 */
export const UMB_DUPLICATE_TO_MODAL = new UmbModalToken<UmbDuplicateToModalData, UmbDuplicateToModalValue>(
	UMB_DUPLICATE_TO_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
