import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import type { UmbDataTypePickerModalData, UmbDataTypePickerModalValue } from '../../modals/constants.js';
import { UMB_DATA_TYPE_PICKER_MODAL } from '../../modals/constants.js';
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
