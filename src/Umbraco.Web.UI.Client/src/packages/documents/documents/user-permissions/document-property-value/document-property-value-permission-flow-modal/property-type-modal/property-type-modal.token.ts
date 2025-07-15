import { UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS } from './manifests.js';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentPropertyValueUserPermissionFlowPropertyTypeModalData {
	documentType: {
		unique: string;
	};
	preset?: Partial<UmbDocumentPropertyValueUserPermissionFlowPropertyTypeModalValue>;
	pickableFilter?: (propertyType: UmbPropertyTypeModel) => boolean;
}

export interface UmbDocumentPropertyValueUserPermissionFlowPropertyTypeModalValue {
	propertyType: {
		unique: string;
	};
	verbs: Array<string>;
}

export const UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL = new UmbModalToken<
	UmbDocumentPropertyValueUserPermissionFlowPropertyTypeModalData,
	UmbDocumentPropertyValueUserPermissionFlowPropertyTypeModalValue
>(UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
