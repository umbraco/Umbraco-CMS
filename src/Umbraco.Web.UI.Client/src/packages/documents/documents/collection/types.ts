import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbDocumentCollectionFilterModel extends UmbCollectionFilterModel {
	unique: string;
	dataTypeId?: string;
	orderBy?: string;
	orderCulture?: string;
	orderDirection?: 'asc' | 'desc';
	userDefinedProperties: Array<{alias: string, header: string, isSystem: boolean}>;
}

export interface UmbDocumentCollectionItemModel {
	unique: string;
	createDate: Date;
	creator?: string | null;
	icon: string;
	name: string;
	state: string;
	updateDate: Date;
	updater?: string | null;
	values: Array<{ alias: string; value: string }>;
}
