import type { UmbBlockGridLayoutModel } from '../../types.js';
import type { UmbBlockDataModel, UmbBlockExposeModel } from '@umbraco-cms/backoffice/block';

// TODO: add the missing fields to the type
export type UmbBlockGridPropertyEditorConfig = Array<{
	alias: 'blocks';
	value: Array<{ allowAtRoot: boolean; contentElementTypeKey: string }>;
}>;

export interface UmbBlockGridPropertyEditorUiValue {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	expose: Array<UmbBlockExposeModel>;
	layout: { [key: string]: Array<UmbBlockGridLayoutModel> | undefined };
}
