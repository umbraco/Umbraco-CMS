import type { UmbDocumentBlueprintEntityType } from './entity.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { DocumentVariantStateModel as UmbDocumentBlueprintVariantState } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDocumentValueModel, UmbDocumentVariantModel } from '@umbraco-cms/backoffice/document';
export { UmbDocumentBlueprintVariantState };

export interface UmbDocumentBlueprintDetailModel {
	documentType: {
		unique: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbDocumentBlueprintEntityType;
	unique: string;
	values: Array<UmbDocumentValueModel>;
	variants: Array<UmbDocumentVariantModel>;
}
