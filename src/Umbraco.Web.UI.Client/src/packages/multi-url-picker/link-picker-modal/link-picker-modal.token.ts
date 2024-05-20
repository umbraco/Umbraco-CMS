import { UMB_MULTI_URL_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbLinkPickerLink } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLinkPickerModalData {
	config: UmbLinkPickerConfig;
	index: number | null;
}

export type UmbLinkPickerModalValue = { link: UmbLinkPickerLink };

// TODO: investigate: this looks more like a property editor configuration. Is this used in the correct way?
export interface UmbLinkPickerConfig {
	hideAnchor?: boolean;
}

export const UMB_LINK_PICKER_MODAL = new UmbModalToken<UmbLinkPickerModalData, UmbLinkPickerModalValue>(
	UMB_MULTI_URL_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
