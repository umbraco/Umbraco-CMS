import type { UmbLanguageCollectionFilterModel, UmbLanguageDetailModel } from '../../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';

export type UmbLanguageCollectionDataSource = UmbCollectionDataSource<
	UmbLanguageDetailModel,
	UmbLanguageCollectionFilterModel
>;
