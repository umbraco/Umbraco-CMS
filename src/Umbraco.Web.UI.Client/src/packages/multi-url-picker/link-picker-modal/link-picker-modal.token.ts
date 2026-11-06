import { UMB_MULTI_URL_PICKER_MODAL_ALIAS } from './constants.js';
import type { UmbLinkPickerLink } from './types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLinkPickerModalData {
	config: UmbLinkPickerConfig;
	index: number | null;
	isNew: boolean;
}

export type UmbLinkPickerModalValue = { link: UmbLinkPickerLink };

export interface UmbLinkPickerDocumentLinksConfig {
	allowCultureSpecificLinks?: boolean;
}

export interface UmbLinkPickerConfig {
	hideAnchor?: boolean;
	hideTarget?: boolean;
	/**
	 * Prevents the name from being filled in from the picked item or the entered URL, so it only holds what the user entered.
	 * For example, a consumer that uses the name as the link's title rather than as its text.
	 */
	disableNameAutofill?: boolean;
	documentLinksConfig?: UmbLinkPickerDocumentLinksConfig;
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
