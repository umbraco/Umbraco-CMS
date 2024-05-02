import { UmbModalToken } from '../../../core/modal/token/modal-token.js';
import type { UmbLinkPickerLink } from './types.js';

export interface UmbLinkPickerModalData {
	config: UmbLinkPickerConfig;
	index: number | null;
}

export type UmbLinkPickerModalValue = { link: UmbLinkPickerLink };

// TODO: investigate: this looks more like a property editor configuration. Is this used in the correct way?
export interface UmbLinkPickerConfig {
	hideAnchor?: boolean;
	ignoreUserStartNodes?: boolean;
}

export const UMB_LINK_PICKER_MODAL = new UmbModalToken<UmbLinkPickerModalData, UmbLinkPickerModalValue>(
	'Umb.Modal.LinkPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
