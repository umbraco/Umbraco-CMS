import {
	UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
} from './constants.js';
import type { ManifestDynamicRootOrigin, ManifestDynamicRootQueryStep } from '@umbraco-cms/backoffice/dynamic-root';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentDynamicRootOriginPickerModalData {
	items: Array<ManifestDynamicRootOrigin>;
}

export interface UmbDocumentDynamicRootQueryStepPickerModalData {
	items: Array<ManifestDynamicRootQueryStep>;
}

export const UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL =
	new UmbModalToken<UmbDocumentDynamicRootOriginPickerModalData>(UMB_DOCUMENT_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS, {
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	});

export const UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL =
	new UmbModalToken<UmbDocumentDynamicRootQueryStepPickerModalData>(
		UMB_DOCUMENT_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
		{
			modal: {
				type: 'sidebar',
				size: 'small',
			},
		},
	);
