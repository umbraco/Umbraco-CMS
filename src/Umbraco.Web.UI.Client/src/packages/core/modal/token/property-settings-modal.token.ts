import { PropertyTypeResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertySettingsModalData = PropertyTypeResponseModelBaseModel;
export type UmbPropertySettingsModalResult = PropertyTypeResponseModelBaseModel;

export const UMB_PROPERTY_SETTINGS_MODAL = new UmbModalToken<
	UmbPropertySettingsModalData,
	UmbPropertySettingsModalResult
>('Umb.Modal.PropertySettings', {
	type: 'sidebar',
	size: 'small',
});
