import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentBlueprintOptionsCreateModalData {
	parent: UmbEntityModel;
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
