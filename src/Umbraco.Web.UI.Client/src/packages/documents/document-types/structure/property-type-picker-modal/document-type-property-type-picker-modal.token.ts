import { UMB_DOCUMENT_TYPE_PROPERTY_TYPE_PICKER_MODAL_ALIAS } from './constants.js';
import { UmbModalToken, type UmbPickerModalData, type UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentTypePropertyTypePickerModalData extends UmbPickerModalData<any> {
	documentType: {
		unique: string;
	};
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentTypePropertyTypePickerModalValue extends UmbPickerModalValue {}

export const UMB_DOCUMENT_TYPE_PROPERTY_PICKER_MODAL = new UmbModalToken<
	UmbDocumentTypePropertyTypePickerModalData,
	UmbDocumentTypePropertyTypePickerModalValue
>(UMB_DOCUMENT_TYPE_PROPERTY_TYPE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
