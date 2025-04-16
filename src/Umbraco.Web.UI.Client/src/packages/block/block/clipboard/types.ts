import type { UmbBlockDataModel, UmbBlockLayoutBaseModel } from '../types.js';

export interface UmbBlockClipboardEntryValueModel {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	layout: Array<UmbBlockLayoutBaseModel> | undefined;
}
