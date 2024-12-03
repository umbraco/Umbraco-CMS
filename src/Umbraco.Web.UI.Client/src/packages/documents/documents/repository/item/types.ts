import type { UmbDocumentEntityType } from '../../entity.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbDocumentItemModel {
	documentType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	entityType: UmbDocumentEntityType;
	hasChildren: boolean;
	isProtected: boolean;
	isTrashed: boolean;
	name: string; // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
	parent: { unique: UmbEntityUnique } | null; // TODO: Use UmbReferenceByUnique when it support unique as null
	unique: string;
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbDocumentItemVariantModel {
	name: string;
	culture: string | null;
	state: DocumentVariantStateModel | null;
}
