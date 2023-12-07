import { UMB_USER_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbUserDetailModel } from '../../types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_USER_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbUserPickerContext extends UmbPickerInputContext<UmbUserDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_ITEM_REPOSITORY_ALIAS, UMB_USER_PICKER_MODAL);
	}
}
