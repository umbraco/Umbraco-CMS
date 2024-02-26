import type { UmbLanguageDetailModel } from '../../types.js';
import type { UmbLanguageCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbLanguageCollectionDataSource = UmbCollectionDataSource<
	UmbLanguageDetailModel,
	UmbLanguageCollectionFilterModel
>;
