import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_REPOSITORY_ALIAS } from '../item/constants.js';
import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_PICKER_MODAL } from '../picker-modal/constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPropertyEditorDataSourcePickerInputContext extends UmbPickerInputContext<UmbPropertyEditorDataSourceItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_REPOSITORY_ALIAS, UMB_PROPERTY_EDITOR_DATA_SOURCE_PICKER_MODAL);
	}
}
