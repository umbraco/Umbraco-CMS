import type { ElementLoaderProperty } from '@umbraco-cms/backoffice/extension-api';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { UUIModalElement, UUIModalSidebarSize } from '@umbraco-cms/backoffice/external/uui';

export type * from './extensions/types.js';

export interface UmbPickerModalData<ItemType> {
	multiple?: boolean;
	filter?: (item: ItemType) => boolean;
	pickableFilter?: (item: ItemType) => boolean;
	search?: UmbPickerModalSearchConfig;
	interactionMemories?: Array<UmbInteractionMemoryModel>;
}

export interface UmbPickerModalSearchConfig<QueryParamsType = Record<string, unknown>> {
	providerAlias: string;
	queryParams?: QueryParamsType;
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
}
