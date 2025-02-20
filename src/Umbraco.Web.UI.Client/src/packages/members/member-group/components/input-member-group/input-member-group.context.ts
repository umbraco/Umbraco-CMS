import type { UmbMemberGroupItemModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_MEMBER_GROUP_PICKER_MODAL } from '../member-group-picker-modal/member-group-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberGroupPickerInputContext extends UmbPickerInputContext<UmbMemberGroupItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_GROUP_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_GROUP_PICKER_MODAL);
	}
}

/** @deprecated Use `UmbMemberGroupPickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbMemberGroupPickerInputContext as UmbMemberPickerContext };
