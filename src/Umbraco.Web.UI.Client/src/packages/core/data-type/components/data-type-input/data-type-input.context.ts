import { DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DATA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDataTypePickerContext extends UmbPickerInputContext<DataTypeItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, DATA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DATA_TYPE_PICKER_MODAL);
	}
}
