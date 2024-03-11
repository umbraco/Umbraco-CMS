import type { UmbDocumentVariantPickerModalData, UmbDocumentVariantPickerModalValue } from '../variant-picker/index.js';
import { UMB_DOCUMENT_PUBLISH_MODAL_ALIAS } from '../manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentPublishModalData extends UmbDocumentVariantPickerModalData {
	allowScheduledPublish?: boolean;
}

export interface UmbDocumentPublishModalValue extends UmbDocumentVariantPickerModalValue {}

export const UMB_DOCUMENT_PUBLISH_MODAL = new UmbModalToken<UmbDocumentPublishModalData, UmbDocumentPublishModalValue>(
	UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
