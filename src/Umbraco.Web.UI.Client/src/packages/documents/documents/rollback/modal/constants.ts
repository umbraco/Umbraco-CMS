import type { UmbRollbackModalData, UmbRollbackModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/** @deprecated No longer used internally. Scheduled for removal in Umbraco 19. */
export const UMB_ROLLBACK_MODAL_ALIAS = 'Umb.Modal.Rollback';

/** @deprecated No longer used internally. Scheduled for removal in Umbraco 19. */
export const UMB_ROLLBACK_MODAL = new UmbModalToken<UmbRollbackModalData, UmbRollbackModalValue>(
	UMB_ROLLBACK_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'full',
		},
	},
);
