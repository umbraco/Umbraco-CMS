import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export type * from './item/types.js';

export interface UmbDocumentCollectionFilterModel extends UmbCollectionFilterModel {
	unique: string;
	dataTypeId?: string;
	/** The data type the request originates from, such as a picker browsing the collection. */
	originDataTypeId?: string;
	orderBy?: string;
	orderCulture?: string;
	orderDirection?: 'asc' | 'desc';
	userDefinedProperties: Array<{ alias: string; header: string; isSystem: boolean }>;
}
