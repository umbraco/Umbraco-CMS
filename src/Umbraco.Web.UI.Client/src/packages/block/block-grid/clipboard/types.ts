import type { UmbBlockGridLayoutModel } from '../types.js';
import type { UmbBlockDataModel, UmbBlockExposeModel } from '@umbraco-cms/backoffice/block';

export interface UmbGridBlockClipboardEntryValueModel {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	expose: Array<UmbBlockExposeModel>;
	layout: UmbBlockGridLayoutModel[] | undefined;
}
