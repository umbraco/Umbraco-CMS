import type { UmbRelationTypeCollectionItemModel, UmbRelationTypeCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbRelationTypeCollectionDataSource = UmbCollectionDataSource<
	UmbRelationTypeCollectionItemModel,
	UmbRelationTypeCollectionFilterModel
>;
