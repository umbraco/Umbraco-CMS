import { UmbModalToken } from '../../../modal/token/modal-token.js';
import type { UmbPropertyTypeModel, UmbPropertyTypeScaffoldModel } from '@umbraco-cms/backoffice/content-type';

export type UmbPropertyTypeSettingsModalData = {
	contentTypeId: string;
};
export type UmbPropertyTypeSettingsModalValue = UmbPropertyTypeModel | UmbPropertyTypeScaffoldModel;

export const UMB_PROPERTY_TYPE_SETTINGS_MODAL = new UmbModalToken<
	UmbPropertyTypeSettingsModalData,
	UmbPropertyTypeSettingsModalValue
>('Umb.Modal.PropertyTypeSettings', {
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
