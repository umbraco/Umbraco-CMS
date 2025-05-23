import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentPublish';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPublishModalData extends UmbDocumentVariantPickerData {
	headline?: string;
	confirmLabel?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPublishModalValue extends UmbDocumentVariantPickerValue {}

export const UMB_DOCUMENT_PUBLISH_MODAL = new UmbModalToken<UmbDocumentPublishModalData, UmbDocumentPublishModalValue>(
	UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
