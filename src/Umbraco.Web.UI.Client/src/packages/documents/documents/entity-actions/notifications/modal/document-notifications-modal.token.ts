import { UMB_DOCUMENT_NOTIFICATIONS_MODAL_ALIAS } from './manifests.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentNotificationsModalData {
	unique: UmbEntityUnique;
}

export const UMB_DOCUMENT_NOTIFICATIONS_MODAL = new UmbModalToken<UmbDocumentNotificationsModalData, never>(
	UMB_DOCUMENT_NOTIFICATIONS_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
