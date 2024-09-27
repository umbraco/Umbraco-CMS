import type { UmbBlockDataModel, UmbBlockExposeModel } from '@umbraco-cms/backoffice/block';

export interface UmbRteBlockValueType {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	expose: Array<UmbBlockExposeModel>;
}

export interface UmbPropertyEditorUiValueType {
	markup: string;
	blocks: UmbRteBlockValueType;
}
