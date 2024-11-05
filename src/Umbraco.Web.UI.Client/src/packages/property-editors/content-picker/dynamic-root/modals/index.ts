import type { ManifestDynamicRootOrigin, ManifestDynamicRootQueryStep } from '../dynamic-root.extension.js';
import {
	UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
} from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbContentPickerDocumentRootOriginModalData {
	items: Array<ManifestDynamicRootOrigin>;
}

export interface UmbContentPickerDocumentRootQueryStepModalData {
	items: Array<ManifestDynamicRootQueryStep>;
}

export const UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL =
	new UmbModalToken<UmbContentPickerDocumentRootOriginModalData>(
		UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
		{
			modal: {
				type: 'sidebar',
				size: 'small',
			},
		},
	);

export const UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL =
	new UmbModalToken<UmbContentPickerDocumentRootQueryStepModalData>(
		UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
		{
			modal: {
				type: 'sidebar',
				size: 'small',
			},
		},
	);
