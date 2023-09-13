import { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertySettingsModalData = {
	documentTypeId: string;
	propertyData: PropertyTypeModelBaseModel
};
export type UmbPropertySettingsModalResult = PropertyTypeModelBaseModel;

export const UMB_PROPERTY_SETTINGS_MODAL = new UmbModalToken<
	UmbPropertySettingsModalData,
	UmbPropertySettingsModalResult
>('Umb.Modal.PropertySettings', {
	type: 'sidebar',
	size: 'small',
});
