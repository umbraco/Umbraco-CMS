import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export type * from './property-data-source.extension.js';

export interface UmbPropertyEditorDataSource extends UmbApi {
	setConfig(config: any): void;
	getConfig(config: any): any;
}

export interface UmbPickerPropertyEditorDataSource
	extends UmbPropertyEditorDataSource,
		UmbItemRepository<any>,
		UmbSearchRepository<any>,
		UmbApi {}

export interface UmbPickerPropertyEditorTreeDataSource
	extends UmbPickerPropertyEditorDataSource,
		UmbTreeRepository,
		UmbApi {}

export interface UmbPickerPropertyEditorCollectionDataSource
	extends UmbPickerPropertyEditorDataSource,
		UmbCollectionRepository,
		UmbApi {}
