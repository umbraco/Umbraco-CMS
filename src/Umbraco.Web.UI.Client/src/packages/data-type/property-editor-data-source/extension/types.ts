import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-data-source.extension.js';

export interface UmbPropertyEditorDataSource extends UmbApi {
	setConfig(config: any): void;
	getConfig(config: any): any;
}

export interface UmbPickerPropertyEditorDataSource extends UmbPropertyEditorDataSource, UmbApi {
	// TODO: Change 'any' to a more specific type
	requestItems(args: any): Promise<any>;
	requestSearch?(args: any): Promise<any>;
}

export interface UmbPickerPropertyEditorTreeDataSource extends UmbPickerPropertyEditorDataSource, UmbApi {
	// TODO: Change 'any' to a more specific type
	requestTreeRoot(args: any): Promise<any>;
	requestTreeRootItems(args: any): Promise<any>;
	requestTreeItemsOf(args: any): Promise<any>;
	requestTreeItemAncestors(args: any): Promise<any>;
}

export interface UmbPickerPropertyEditorCollectionDataSource extends UmbPickerPropertyEditorDataSource, UmbApi {
	// TODO: Change 'any' to a more specific type
	requestCollection(args: any): Promise<any>;
}
