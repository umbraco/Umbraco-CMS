import { UMB_MULTI_URL_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbLinkPickerLink } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLinkPickerModalData {
	config: UmbLinkPickerConfig;
	index: number | null;
	isNew: boolean;
}

export type UmbLinkPickerModalValue = { link: UmbLinkPickerLink };

export interface UmbLinkPickerConfig {
	hideAnchor?: boolean;
	hideTarget?: boolean;
}

export const UMB_LINK_PICKER_MODAL = new UmbModalToken<UmbLinkPickerModalData, UmbLinkPickerModalValue>(
	UMB_MULTI_URL_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
