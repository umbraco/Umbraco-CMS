import { UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS, type UmbUserGroupItemModel } from '../../repository/index.js';
import { UMB_USER_GROUP_PICKER_MODAL } from '../../modals/user-group-picker/user-group-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupPickerContext extends UmbPickerInputContext<UmbUserGroupItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_USER_GROUP_ITEM_REPOSITORY_ALIAS, UMB_USER_GROUP_PICKER_MODAL);
	}
}
