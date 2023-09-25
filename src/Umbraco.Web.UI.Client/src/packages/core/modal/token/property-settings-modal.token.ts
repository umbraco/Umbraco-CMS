import { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertySettingsModalData = {
	documentTypeId: string;
	propertyData: PropertyTypeModelBaseModel;
};
export type UmbPropertySettingsModalValue = PropertyTypeModelBaseModel;

export const UMB_PROPERTY_SETTINGS_MODAL = new UmbModalToken<
	UmbPropertySettingsModalData,
	UmbPropertySettingsModalValue
>('Umb.Modal.PropertySettings', {
	type: 'sidebar',
	size: 'small',
});
