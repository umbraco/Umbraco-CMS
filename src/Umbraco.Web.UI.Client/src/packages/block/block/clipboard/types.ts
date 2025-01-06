import type { UmbBlockDataModel, UmbBlockExposeModel, UmbBlockLayoutBaseModel } from '../types.js';

export interface UmbBlockClipboardEntryValueModel {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	expose: Array<UmbBlockExposeModel>;
	layout: Array<UmbBlockLayoutBaseModel> | undefined;
}
