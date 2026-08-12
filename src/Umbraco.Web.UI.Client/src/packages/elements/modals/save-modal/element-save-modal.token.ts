import type { UmbElementVariantPickerData, UmbElementVariantPickerValue } from '../types.js';
import { UMB_ELEMENT_SAVE_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementSaveModalData extends UmbElementVariantPickerData {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementSaveModalValue extends UmbElementVariantPickerValue {}

export const UMB_ELEMENT_SAVE_MODAL = new UmbModalToken<UmbElementSaveModalData, UmbElementSaveModalValue>(
	UMB_ELEMENT_SAVE_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
