import type { UmbDocumentEntityType } from '../../entity.js';
import type { UmbDocumentItemVariantModel } from '../../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';

export interface UmbDocumentCollectionItemModel extends UmbEntityWithFlags {
	ancestors: Array<UmbEntityModel>;
	creator?: string | null;
	documentType: {
		unique: string;
		icon: string;
		alias: string;
	};
	entityType: UmbDocumentEntityType;
	isProtected: boolean;
	isTrashed: boolean;
	sortOrder: number;
	unique: string;
	updater?: string | null;
	values: Array<{ alias: string; culture?: string; segment?: string; value: string }>;
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbEditableDocumentCollectionItemModel {
	item: UmbDocumentCollectionItemModel;
	editPath: string;
}
