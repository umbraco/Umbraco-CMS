import type { UmbRollbackModalData, UmbRollbackModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_ROLLBACK_MODAL_ALIAS = 'Umb.Modal.Rollback';

export const UMB_ROLLBACK_MODAL = new UmbModalToken<UmbRollbackModalData, UmbRollbackModalValue>(
	UMB_ROLLBACK_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'full',
		},
	},
);
