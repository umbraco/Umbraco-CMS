import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../types.js';
import { UMB_DOCUMENT_SAVE_MODAL_ALIAS } from '../manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentSaveModalData extends UmbDocumentVariantPickerData {}

export interface UmbDocumentSaveModalValue extends UmbDocumentVariantPickerValue {}

export const UMB_DOCUMENT_SAVE_MODAL = new UmbModalToken<UmbDocumentSaveModalData, UmbDocumentSaveModalValue>(
	UMB_DOCUMENT_SAVE_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
