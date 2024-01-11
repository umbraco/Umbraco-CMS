import { UMB_RENAME_MODAL_ALIAS } from './manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbRenameModalData {
	unique: string;
}

export interface UmbRenameModalValue {}

export const UMB_RENAME_MODAL = new UmbModalToken<UmbRenameModalData, UmbRenameModalValue>(UMB_RENAME_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
