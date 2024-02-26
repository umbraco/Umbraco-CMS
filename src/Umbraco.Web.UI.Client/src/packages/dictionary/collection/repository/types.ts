import type { UmbDictionaryCollectionFilterModel, UmbDictionaryCollectionModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbDictionaryCollectionDataSource = UmbCollectionDataSource<
	UmbDictionaryCollectionModel,
	UmbDictionaryCollectionFilterModel
>;
