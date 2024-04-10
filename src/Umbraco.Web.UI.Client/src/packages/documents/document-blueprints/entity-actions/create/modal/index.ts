import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentBlueprintCreateOptionsModalData {
	parent: {
		unique: string | null;
		entityType: string;
	};
}

export interface UmbDocumentBlueprintCreateOptionsModalValue {
	documentTypeUnique: string;
}

export const UMB_DOCUMENT_BLUEPRINT_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbDocumentBlueprintCreateOptionsModalData,
	UmbDocumentBlueprintCreateOptionsModalValue
>('Umb.Modal.DocumentBlueprintCreateOptions', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
