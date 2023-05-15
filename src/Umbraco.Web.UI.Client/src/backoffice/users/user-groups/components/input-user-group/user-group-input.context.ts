import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbUserGroupPickerContext extends UmbPickerInputContext<UserGroupItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.UserGroup', UMB_USER_GROUP_PICKER_MODAL);
	}
}
