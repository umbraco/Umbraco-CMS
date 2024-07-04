import { UmbModalToken } from '../../../modal/token/modal-token.js';

export type UmbPropertyTypeSettingsModalData = {
	contentTypeUnique: string;
};

export const UMB_PROPERTY_TYPE_SETTINGS_MODAL = new UmbModalToken<UmbPropertyTypeSettingsModalData, undefined>(
	'Umb.Modal.PropertyTypeSettings',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
