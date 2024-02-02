import { UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS, type UmbUserGroupItemModel } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbUserGroupPickerContext extends UmbPickerInputContext<UmbUserGroupItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS, UMB_USER_GROUP_PICKER_MODAL);
	}
}
