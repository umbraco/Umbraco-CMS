import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export interface UmbCollectionItemPickerModalData<CollectionItemType = UmbEntityModel>
	extends UmbPickerModalData<CollectionItemType> {
	collection: UmbCollectionItemPickerModalCollectionConfig;
}

export interface UmbCollectionItemPickerModalCollectionConfig<FilterArgsType = Record<string, unknown>> {
	alias?: string;
	menuAlias?: string;
	filterArgs?: FilterArgsType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbCollectionItemPickerModalValue extends UmbPickerModalValue {}
