import { UMB_USER_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbUserDetailModel } from '../../types.js';
import { UMB_USER_PICKER_MODAL } from '../../modals/user-picker/user-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserPickerInputContext extends UmbPickerInputContext<UmbUserDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_ITEM_REPOSITORY_ALIAS, UMB_USER_PICKER_MODAL);
	}
}
