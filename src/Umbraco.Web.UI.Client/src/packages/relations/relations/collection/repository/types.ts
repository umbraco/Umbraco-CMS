import type { UmbRelationDetailModel } from '../../types.js';
import type { UmbRelationCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbRelationCollectionDataSource = UmbCollectionDataSource<
	UmbRelationDetailModel,
	UmbRelationCollectionFilterModel
>;
