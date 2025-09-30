import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export type * from './picker-property-editor-collection-data-source.extension.js';
export type * from './picker-property-editor-tree-data-source.extension.js';

// TODO: should these interface be renamed to Repository instead of DataSource?
export interface UmbPickerPropertyEditorTreeDataSource extends UmbApi {
	tree: UmbTreeRepository;
	// TODO: Change 'any' to a more specific type
	item: UmbItemRepository<any>;
	// TODO: Change 'any' to a more specific type
	search?: UmbSearchRepository<any>;
}

export interface UmbPickerPropertyEditorCollectionDataSource extends UmbApi {
	collection: UmbCollectionRepository;
	// TODO: Change 'any' to a more specific type
	item: UmbItemRepository<any>;
	// TODO: Change 'any' to a more specific type
	search?: UmbSearchRepository<any>;
}
