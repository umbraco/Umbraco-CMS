import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_USER_GROUP_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbUserGroupPickerContext extends UmbPickerInputContext<UserItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.UserGroup', UMB_USER_GROUP_PICKER_MODAL);
	}
}
