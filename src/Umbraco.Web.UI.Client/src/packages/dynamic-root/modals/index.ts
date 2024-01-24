import {
	UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
} from './manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL = new UmbModalToken(UMB_DYNAMIC_ROOT_ORIGIN_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});

export const UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL = new UmbModalToken(
	UMB_DYNAMIC_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
