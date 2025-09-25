import type { UmbDocumentEntityType } from '../entity.js';
import type { UmbDocumentItemVariantModel } from '../item/repository/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbDocumentCollectionFilterModel extends UmbCollectionFilterModel {
	unique: string;
	dataTypeId?: string;
	orderBy?: string;
	orderCulture?: string;
	orderDirection?: 'asc' | 'desc';
	userDefinedProperties: Array<{ alias: string; header: string; isSystem: boolean }>;
}

export interface UmbDocumentCollectionItemModel {
	ancestors: Array<UmbEntityModel>;
	unique: string;
	entityType: UmbDocumentEntityType;
	creator?: string | null;
	sortOrder: number;
	updater?: string | null;
	values: Array<{ alias: string; value: string }>;
	isProtected: boolean;
	isTrashed: boolean;
	documentType: {
		unique: string;
		icon: string;
		alias: string;
	};
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbEditableDocumentCollectionItemModel {
	item: UmbDocumentCollectionItemModel;
	editPath: string;
}
