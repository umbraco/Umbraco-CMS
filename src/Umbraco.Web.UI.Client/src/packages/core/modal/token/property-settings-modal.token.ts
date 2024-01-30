import type { UmbPropertyTypeModel, UmbPropertyTypeScaffoldModel } from '@umbraco-cms/backoffice/content-type';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertySettingsModalData = {
	documentTypeId: string;
};
export type UmbPropertySettingsModalValue = UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel;

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
