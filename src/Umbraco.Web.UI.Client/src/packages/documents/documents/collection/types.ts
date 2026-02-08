import type { UmbDocumentEntityType } from '../entity.js';
import type { UmbDocumentItemVariantModel } from '../item/types.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';

export type * from './item/types.js';

export interface UmbDocumentCollectionFilterModel extends UmbCollectionFilterModel {
	unique: string;
	dataTypeId?: string;
	orderBy?: string;
	orderCulture?: string;
	orderDirection?: 'asc' | 'desc';
	userDefinedProperties: Array<{ alias: string; header: string; isSystem: boolean }>;
}

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
	values: Array<UmbDocumentCollectionItemPropertyValueModel>;
	variants: Array<UmbDocumentItemVariantModel>;
}

export interface UmbDocumentCollectionItemPropertyValueModel {
	alias: string;
	editorAlias: string;
	culture?: string;
	segment?: string;
	value: string;
}

export interface UmbEditableDocumentCollectionItemModel {
	item: UmbDocumentCollectionItemModel;
	editPath: string;
}
