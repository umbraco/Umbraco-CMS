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
	/**
	 * Set the unique key of the modal, used to identify the modal instance.
	 */
	key?: string;

	/**
	 * Set how the modal is presented: as a dialog, a sidebar or a custom element.
	 */
	type?: UmbModalType;

	/**
	 * Set the size of the modal, applies to sidebar modals.
	 */
	size?: UUIModalSidebarSize;

	/**
	 * Mark the modal as showing a reference, a bar is displayed in the modal to indicate this.
	 */
	isReference?: boolean;

	/**
	 * Used to provide a custom modal element to replace the default uui-modal-dialog or uui-modal-sidebar.
	 */
	element?: ElementLoaderProperty<UUIModalElement>;

	/**
	 * Set the background property of the modal backdrop.
	 */
	backdropBackground?: string;

	/**
	 * Set the title of the modal, this is used as Browser Title.
	 */
	title?: string;
}
