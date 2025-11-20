import { UMB_TREE_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_TREE_PICKER_MODAL = new UmbModalToken<UmbTreePickerModalData, UmbTreePickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
