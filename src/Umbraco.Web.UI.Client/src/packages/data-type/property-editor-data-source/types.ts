export type * from './extension/types.js';

export interface UmbPropertyEditorDataSourceConfigEntryModel {
	alias: string;
	value: unknown;
}

export type UmbPropertyEditorDataSourceConfigModel = Array<UmbPropertyEditorDataSourceConfigEntryModel>;
