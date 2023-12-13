import { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertySettingsModalData = {
	documentTypeId: string;
};
export type UmbPropertySettingsModalValue = PropertyTypeModelBaseModel;

export const UMB_PROPERTY_SETTINGS_MODAL = new UmbModalToken<
	UmbPropertySettingsModalData,
	UmbPropertySettingsModalValue
>('Umb.Modal.PropertySettings', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	value: {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		validation: {},
	},
});
