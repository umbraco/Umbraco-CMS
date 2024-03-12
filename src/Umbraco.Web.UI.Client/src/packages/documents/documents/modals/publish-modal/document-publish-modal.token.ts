import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../types.js';
import { UMB_DOCUMENT_PUBLISH_MODAL_ALIAS } from '../manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { ScheduleRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbDocumentPublishModalData extends UmbDocumentVariantPickerData {
	allowScheduledPublish?: boolean;
}

export interface UmbDocumentPublishModalValue extends UmbDocumentVariantPickerValue {
	schedule?: ScheduleRequestModel;
}

export const UMB_DOCUMENT_PUBLISH_MODAL = new UmbModalToken<UmbDocumentPublishModalData, UmbDocumentPublishModalValue>(
	UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
