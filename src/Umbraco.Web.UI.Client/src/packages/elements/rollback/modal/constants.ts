import type { UmbElementRollbackModalData, UmbElementRollbackModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_ELEMENT_ROLLBACK_MODAL_ALIAS = 'Umb.Modal.Element.Rollback';

export const UMB_ELEMENT_ROLLBACK_MODAL = new UmbModalToken<UmbElementRollbackModalData, UmbElementRollbackModalValue>(
	UMB_ELEMENT_ROLLBACK_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'full',
		},
	},
);
