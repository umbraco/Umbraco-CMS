import type { UmbContentRollbackModalData, UmbContentRollbackModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_CONTENT_ROLLBACK_MODAL_ALIAS = 'Umb.Modal.Content.Rollback';

export const UMB_CONTENT_ROLLBACK_MODAL = new UmbModalToken<UmbContentRollbackModalData, UmbContentRollbackModalValue>(
	UMB_CONTENT_ROLLBACK_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'full',
		},
	},
);
