import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentPropertyValueGranularUserPermissionFlowModalData {
	preset?: Partial<UmbDocumentPropertyValueGranularUserPermissionFlowModalValue>;
}

export interface UmbDocumentPropertyValueGranularUserPermissionFlowModalValue {
	documentType: {
		unique: string;
	};
	propertyType: {
		unique: string;
	};
	verbs: Array<string>;
}

export const UMB_DOCUMENT_PROPERTY_VALUE_GRANULAR_USER_PERMISSION_FLOW_MODAL = new UmbModalToken<
	UmbDocumentPropertyValueGranularUserPermissionFlowModalData,
	UmbDocumentPropertyValueGranularUserPermissionFlowModalValue
>('Umb.Modal.DocumentPropertyValueGranularUserPermissionFlow', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
