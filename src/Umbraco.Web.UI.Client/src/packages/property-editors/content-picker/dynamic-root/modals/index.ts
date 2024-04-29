import {
	UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
} from './manifests.js';
import type {
	ManifestDynamicRootOrigin,
	ManifestDynamicRootQueryStep,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDynamicRootOriginModalData {
	items: Array<ManifestDynamicRootOrigin>;
}

export interface UmbDynamicRootQueryStepModalData {
	items: Array<ManifestDynamicRootQueryStep>;
}

export const UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL = new UmbModalToken<UmbDynamicRootOriginModalData>(
	UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);

export const UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL = new UmbModalToken<UmbDynamicRootQueryStepModalData>(
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
