import type { UmbDocumentEntityType } from '../../entity.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityFlag, UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbDocumentItemModel extends UmbEntityWithFlags {
	documentType: {
		unique: string;
		icon: string;
		collection?: UmbReferenceByUnique | null;
	};
	entityType: UmbDocumentEntityType;
	hasChildren: boolean;
	isProtected: boolean;
	isTrashed: boolean;
	parent: { unique: UmbEntityUnique } | null; // TODO: Use UmbReferenceByUnique when it support unique as null
	unique: string;
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbDocumentItemVariantModel {
	name: string;
	culture: string | null;
	state: DocumentVariantStateModel | null;
	createDate?: Date;
	updateDate?: Date;
	flags: Array<UmbEntityFlag>;
}
