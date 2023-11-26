import { UMB_USER_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_USER_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbUserPickerContext extends UmbPickerInputContext<UserItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_ITEM_REPOSITORY_ALIAS, UMB_USER_PICKER_MODAL);
	}
}
