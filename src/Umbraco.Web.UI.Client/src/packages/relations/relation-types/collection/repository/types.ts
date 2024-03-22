import type { UmbRelationTypeDetailModel } from '../../types.js';
import type { UmbRelationTypeCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbRelationTypeCollectionDataSource = UmbCollectionDataSource<
	UmbRelationTypeDetailModel,
	UmbRelationTypeCollectionFilterModel
>;
