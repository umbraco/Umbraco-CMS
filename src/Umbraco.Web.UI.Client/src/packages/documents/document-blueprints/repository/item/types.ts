import type { UmbDocumentBlueprintEntityType } from '../../entity.js';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbDocumentBlueprintItemModel extends UmbDocumentBlueprintItemBaseModel {
	documentType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
}

export interface UmbDocumentBlueprintItemBaseModel {
	entityType: UmbDocumentBlueprintEntityType;
	name: string;
	unique: string;
}

export interface UmbDocumentBlueprintItemVariantModel {
	name: string;
	culture: string | null;
	state: DocumentVariantStateModel | null;
}
