import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentPropertyValueUserPermissionFlowModalData {
	preset?: Partial<UmbDocumentPropertyValueUserPermissionFlowModalValue>;
	pickablePropertyTypeFilter?: (propertyType: UmbPropertyTypeModel) => boolean;
}

export interface UmbDocumentPropertyValueUserPermissionFlowModalValue {
	documentType: {
		unique: string;
	};
	propertyType: {
		unique: string;
	};
	verbs: Array<string>;
}

export const UMB_DOCUMENT_PROPERTY_VALUE_USER_PERMISSION_FLOW_MODAL = new UmbModalToken<
	UmbDocumentPropertyValueUserPermissionFlowModalData,
	UmbDocumentPropertyValueUserPermissionFlowModalValue
>('Umb.Modal.DocumentPropertyValueUserPermissionFlow', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
