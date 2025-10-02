import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './picker-property-editor-collection-data-source.extension.js';
export type * from './picker-property-editor-tree-data-source.extension.js';

// TODO: should these interface be renamed to Repository instead of DataSource?

export interface UmbPickerPropertyEditorDataSource extends UmbApi {
	setConfig(config: any): void;
	getConfig(config: any): any;

	// TODO: Change 'any' to a more specific type
	// Items
	requestItems(args: any): Promise<any>;

	// Search
	requestSearch?(args: any): Promise<any>;
}

export interface UmbPickerPropertyEditorTreeDataSource extends UmbPickerPropertyEditorDataSource, UmbApi {
	// TODO: Change 'any' to a more specific type
	// Tree
	requestTreeRoot(args: any): Promise<any>;
	requestTreeRootItems(args: any): Promise<any>;
	requestTreeItemsOf(args: any): Promise<any>;
	requestTreeItemAncestors(args: any): Promise<any>;
}

export interface UmbPickerPropertyEditorCollectionDataSource extends UmbPickerPropertyEditorDataSource, UmbApi {
	// Collection
	requestCollection(args: any): Promise<any>;
}
