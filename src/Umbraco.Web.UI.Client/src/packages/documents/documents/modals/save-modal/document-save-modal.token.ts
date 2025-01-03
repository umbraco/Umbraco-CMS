import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../types.js';
import { UMB_DOCUMENT_SAVE_MODAL_ALIAS } from './manifest.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentSaveModalData extends UmbDocumentVariantPickerData {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentSaveModalValue extends UmbDocumentVariantPickerValue {}

export const UMB_DOCUMENT_SAVE_MODAL = new UmbModalToken<UmbDocumentSaveModalData, UmbDocumentSaveModalValue>(
	UMB_DOCUMENT_SAVE_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
