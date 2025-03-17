import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentValueGranularUserPermissionFlowModalData {
	preset?: Partial<UmbDocumentValueGranularUserPermissionFlowModalValue>;
}

export interface UmbDocumentValueGranularUserPermissionFlowModalValue {
	documentType: {
		unique: string;
	};
	propertyType: {
		unique: string;
	};
	verbs: Array<string>;
}

export const UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_MODAL = new UmbModalToken<
	UmbDocumentValueGranularUserPermissionFlowModalData,
	UmbDocumentValueGranularUserPermissionFlowModalValue
>('Umb.Modal.DocumentValueGranularUserPermissionFlow', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
