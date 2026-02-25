import type { UmbRollbackModalData, UmbRollbackModalValue } from '@umbraco-cms/backoffice/content';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_ROLLBACK_MODAL_ALIAS = 'Umb.Modal.Document.Rollback';

export const UMB_DOCUMENT_ROLLBACK_MODAL = new UmbModalToken<UmbRollbackModalData, UmbRollbackModalValue>(
	UMB_DOCUMENT_ROLLBACK_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'full',
		},
	},
);

/** @deprecated Use {@link UMB_DOCUMENT_ROLLBACK_MODAL_ALIAS} instead. Scheduled for removal in Umbraco 19. */
export const UMB_ROLLBACK_MODAL_ALIAS = UMB_DOCUMENT_ROLLBACK_MODAL_ALIAS;

/** @deprecated Use {@link UMB_DOCUMENT_ROLLBACK_MODAL} instead. Scheduled for removal in Umbraco 19. */
export const UMB_ROLLBACK_MODAL = UMB_DOCUMENT_ROLLBACK_MODAL;
