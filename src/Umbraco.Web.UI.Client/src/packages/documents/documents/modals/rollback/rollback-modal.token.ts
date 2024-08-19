import { UMB_ROLLBACK_MODAL_ALIAS } from '../manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRollbackModalData {}
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbRollbackModalValue {}

export const UMB_ROLLBACK_MODAL = new UmbModalToken<UmbRollbackModalData, UmbRollbackModalValue>(
	UMB_ROLLBACK_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'full',
		},
	},
);
