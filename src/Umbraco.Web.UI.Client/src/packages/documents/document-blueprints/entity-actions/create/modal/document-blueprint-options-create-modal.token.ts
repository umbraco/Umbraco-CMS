import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentBlueprintOptionsCreateModalData {
	parent: {
		unique: string | null;
		entityType: string;
	};
}

export interface UmbDocumentBlueprintOptionsCreateModalValue {
	documentTypeUnique: string;
}

export const UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL = new UmbModalToken<
	UmbDocumentBlueprintOptionsCreateModalData,
	UmbDocumentBlueprintOptionsCreateModalValue
>('Umb.Modal.DocumentBlueprintOptionsCreate', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
