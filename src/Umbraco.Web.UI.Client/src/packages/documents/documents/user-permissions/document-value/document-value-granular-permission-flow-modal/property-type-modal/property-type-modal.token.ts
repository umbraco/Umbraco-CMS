import { UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalData {
	documentType: {
		unique: string;
	};
	preset?: Partial<UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalValue>;
}

export interface UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalValue {
	propertyType: {
		unique: string;
	};
	verbs: Array<string>;
}

export const UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL = new UmbModalToken<
	UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalData,
	UmbDocumentValueGranularUserPermissionFlowPropertyTypeModalValue
>(UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
