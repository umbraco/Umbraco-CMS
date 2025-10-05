import type { UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbEntityDataPickerCollectionItemModel extends UmbCollectionItemModel {
	unique: string;
	name: string;
	icon: string;
}
