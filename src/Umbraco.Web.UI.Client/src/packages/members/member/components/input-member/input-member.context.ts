import type { UmbMemberItemModel } from '../../repository/index.js';
import { UMB_MEMBER_PICKER_MODAL } from '../member-picker-modal/member-picker-modal.token.js';
import { UMB_MEMBER_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberPickerInputContext extends UmbPickerInputContext<UmbMemberItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_PICKER_MODAL);
	}
}

/** @deprecated Use `UmbMemberPickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbMemberPickerInputContext as UmbMemberPickerContext };
