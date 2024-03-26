import { UMB_MEMBER_TYPE_PICKER_MODAL } from '../../modal/member-type-picker-modal.token.js';
import { UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS } from '@umbraco-cms/backoffice/member-type';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberTypePickerContext extends UmbPickerInputContext<any> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_PICKER_MODAL);
	}
}
