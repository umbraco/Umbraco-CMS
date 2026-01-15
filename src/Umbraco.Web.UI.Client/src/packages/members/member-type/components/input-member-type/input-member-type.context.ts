import type {
	UmbMemberTypePickerModalData,
	UmbMemberTypePickerModalValue,
} from '../../modal/member-type-picker-modal.token.js';
import { UMB_MEMBER_TYPE_PICKER_MODAL } from '../../modal/member-type-picker-modal.token.js';
import type { UmbMemberTypeItemModel } from '../../repository/item/types.js';
import type { UmbMemberTypeTreeItemModel } from '../../tree/types.js';
import { UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberTypePickerInputContext extends UmbPickerInputContext<
	UmbMemberTypeItemModel,
	UmbMemberTypeTreeItemModel,
	UmbMemberTypePickerModalData,
	UmbMemberTypePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_PICKER_MODAL);
	}
}
