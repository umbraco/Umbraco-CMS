import type { UmbPropertyEditorDataSourceConfigModel } from '../types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export type * from './property-data-source.extension.js';

export interface UmbPropertyEditorDataSource extends UmbApi {
	setConfig?(config: UmbPropertyEditorDataSourceConfigModel | undefined): void;
	getConfig?(): UmbPropertyEditorDataSourceConfigModel | undefined;
}

// TODO: interfaces to picker/tree/collection etc modules
export interface UmbPickerPropertyEditorDataSource
	extends UmbPropertyEditorDataSource,
		UmbItemRepository<any>,
		UmbApi {}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbSearchablePickerPropertyEditorDataSource extends UmbSearchRepository<any> {}

export interface UmbPickerPropertyEditorTreeDataSource
	extends UmbPickerPropertyEditorDataSource,
		UmbTreeRepository,
		UmbApi {}

export interface UmbPickerPropertyEditorCollectionDataSource
	extends UmbPickerPropertyEditorDataSource,
		UmbCollectionRepository,
		UmbApi {}
