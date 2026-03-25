import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * @deprecated Use the `Umb.EntityAction.DocumentBlueprint.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export interface UmbDocumentBlueprintOptionsCreateModalData {
	parent: UmbEntityModel;
}

/**
 * @deprecated Use the `Umb.EntityAction.DocumentBlueprint.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export interface UmbDocumentBlueprintOptionsCreateModalValue {
	documentTypeUnique: string;
}

/**
 * @deprecated Use the `Umb.EntityAction.DocumentBlueprint.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19.
 */
export const UMB_DOCUMENT_BLUEPRINT_OPTIONS_CREATE_MODAL = new UmbModalToken<
	UmbDocumentBlueprintOptionsCreateModalData,
	UmbDocumentBlueprintOptionsCreateModalValue
>('Umb.Modal.DocumentBlueprintOptionsCreate', {
	modal: {
		type: 'dialog',
	},
});
