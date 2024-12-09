import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../types.js';
import { UMB_DOCUMENT_PUBLISH_MODAL_ALIAS } from './manifest.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPublishModalData extends UmbDocumentVariantPickerData {}

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
