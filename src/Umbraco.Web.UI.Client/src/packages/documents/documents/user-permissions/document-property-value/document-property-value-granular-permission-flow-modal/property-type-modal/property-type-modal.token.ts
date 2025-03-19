import { UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS } from './constants.js';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentPropertyValueGranularUserPermissionFlowPropertyTypeModalData {
	documentType: {
		unique: string;
	};
	preset?: Partial<UmbDocumentPropertyValueGranularUserPermissionFlowPropertyTypeModalValue>;
	pickableFilter?: (propertyType: UmbPropertyTypeModel) => boolean;
}

export interface UmbDocumentPropertyValueGranularUserPermissionFlowPropertyTypeModalValue {
	propertyType: {
		unique: string;
	};
	verbs: Array<string>;
}

export const UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL = new UmbModalToken<
	UmbDocumentPropertyValueGranularUserPermissionFlowPropertyTypeModalData,
	UmbDocumentPropertyValueGranularUserPermissionFlowPropertyTypeModalValue
>(UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
