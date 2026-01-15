import type { UmbBlockGridLayoutModel } from '../types.js';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';

export interface UmbGridBlockClipboardEntryValueModel {
	contentData: Array<UmbBlockDataModel>;
	settingsData: Array<UmbBlockDataModel>;
	layout: UmbBlockGridLayoutModel[] | undefined;
}
