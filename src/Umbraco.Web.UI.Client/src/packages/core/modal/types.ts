import type { ElementLoaderProperty } from '@umbraco-cms/backoffice/extension-api';
import type { UUIModalElement, UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

export type * from './extensions/types.js';

export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
	search?: UmbPickerModalSearchConfig;
}

export interface UmbPickerModalSearchConfig<
	QueryParamsType = Record<string, unknown>,
	SearchResultItemType extends UmbSearchResultItemModel = UmbSearchResultItemModel,
> {
	providerAlias: string;
	queryParams?: QueryParamsType;
	pickableFilter?: (item: SearchResultItemType) => boolean;
}

export interface UmbPickerModalValue {
	selection: Array<string | null>;
}

export type UmbModalType = 'dialog' | 'sidebar' | 'custom';

export interface UmbModalConfig {
	key?: string;
	type?: UmbModalType;
	size?: UUIModalSidebarSize;

	/**
	 * Used to provide a custom modal element to replace the default uui-modal-dialog or uui-modal-sidebar
	 */
	element?: ElementLoaderProperty<UUIModalElement>;

	/**
	 * Set the background property of the modal backdrop
	 */
	backdropBackground?: string;

	/**
	 * Set the title of the modal, this is used as Browser Title
	 */
	title?: string;
}
