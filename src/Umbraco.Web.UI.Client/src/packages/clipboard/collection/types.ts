import type { UmbClipboardEntryDetailModel } from '../clipboard-entry/index.js';
import type { UmbCollectionFilterModel } from '@umbraco-cms/backoffice/collection';

export interface UmbClipboardCollectionFilterModel extends UmbCollectionFilterModel {
	types?: Array<string>;
	asyncFilter?: (entry: UmbClipboardEntryDetailModel) => Promise<boolean>;
}
