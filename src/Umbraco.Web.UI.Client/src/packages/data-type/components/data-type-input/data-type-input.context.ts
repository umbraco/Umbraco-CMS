import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import type { UmbDataTypePickerModalData, UmbDataTypePickerModalValue } from '../../modals/index.js';
import { UMB_DATA_TYPE_PICKER_MODAL } from '../../modals/index.js';
import type { UmbDataTypeTreeItemModel } from '../../tree/types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';

export class UmbDataTypePickerInputContext extends UmbPickerInputContext<
	UmbDataTypeItemModel,
	UmbDataTypeTreeItemModel,
	UmbDataTypePickerModalData,
	UmbDataTypePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DATA_TYPE_PICKER_MODAL);
	}
}

/** @deprecated Use `UmbDataTypePickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbDataTypePickerInputContext as UmbDataTypePickerContext };
