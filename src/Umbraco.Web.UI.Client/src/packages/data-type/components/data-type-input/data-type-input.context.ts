import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import { UMB_DATA_TYPE_PICKER_MODAL } from '../../modals/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDataTypePickerContext extends UmbPickerInputContext<UmbDataTypeItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DATA_TYPE_PICKER_MODAL);
	}
}
