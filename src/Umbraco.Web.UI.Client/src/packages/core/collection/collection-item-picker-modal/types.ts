import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export interface UmbCollectionItemPickerModalData<CollectionItemType = UmbEntityModel>
	extends UmbPickerModalData<CollectionItemType> {
	collectionMenuAlias?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbCollectionItemPickerModalValue extends UmbPickerModalValue {}
