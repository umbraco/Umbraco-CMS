import { UUIModalSidebarSize } from '@umbraco-ui/uui-modal-sidebar';
import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbLinkPickerModalData {
	link: UmbLinkPickerLink;
	config: UmbLinkPickerConfig;
}

export type UmbLinkPickerModalResult = UmbLinkPickerLink;

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

export const UMB_LINK_PICKER_MODAL_TOKEN = new UmbModalToken<UmbLinkPickerModalData, UmbLinkPickerModalResult>(
	'Umb.Modal.LinkPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
