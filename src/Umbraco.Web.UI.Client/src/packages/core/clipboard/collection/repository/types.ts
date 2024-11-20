import type { UmbClipboardEntry } from '../../clipboard-entry/index.js';
import type { UmbClipboardCollectionFilterModel } from '../types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';

export type UmbClipboardCollectionDataSource = UmbCollectionDataSource<
	UmbClipboardEntry,
	UmbClipboardCollectionFilterModel
>;
