import type { UmbDictionaryCollectionFilterModel, UmbDictionaryCollectionModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export type UmbDictionaryCollectionDataSource = UmbCollectionDataSource<
	UmbDictionaryCollectionModel,
	UmbDictionaryCollectionFilterModel
>;
