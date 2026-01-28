import type { UmbCollectionItemModel } from '../item/types.js';

export interface UmbCollectionSelectionManagerConfig {
	multiple?: boolean;
	selectable?: boolean;
	selection?: Array<UmbCollectionItemModel['unique']>;
	selectableFilter?(item: UmbCollectionItemModel): boolean;
}
