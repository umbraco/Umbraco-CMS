import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbUserGroupPickerContext extends UmbPickerInputContext<UserGroupItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.UserGroup', UMB_USER_GROUP_PICKER_MODAL);
	}
}
