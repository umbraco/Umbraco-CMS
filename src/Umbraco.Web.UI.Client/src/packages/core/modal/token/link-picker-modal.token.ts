import type { UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbLinkPickerModalData {
	index: number | null;
	link: UmbLinkPickerLink;
	config: UmbLinkPickerConfig;
}

export type UmbLinkPickerModalValue = { index: number | null; link: UmbLinkPickerLink };

export interface UmbLinkPickerLink {
	icon?: string | null;
	name?: string | null;
	published?: boolean | null;
	queryString?: string | null;
	target?: string | null;
	trashed?: boolean | null;
	udi?: string | null;
	url?: string | null;
}

// TODO: investigate: this looks more like a property editor configuration. Is this used in the correct way?
export interface UmbLinkPickerConfig {
	hideAnchor?: boolean;
	ignoreUserStartNodes?: boolean;
	overlaySize?: UUIModalSidebarSize;
}

export const UMB_LINK_PICKER_MODAL = new UmbModalToken<UmbLinkPickerModalData, UmbLinkPickerModalValue>(
	'Umb.Modal.LinkPicker',
	{
		type: 'sidebar',
		size: 'small',
	},
);
