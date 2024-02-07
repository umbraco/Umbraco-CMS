import { UMB_MEMBER_TYPE_PICKER_MODAL } from '../../modal/member-type-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbMemberTypePickerContext extends UmbPickerInputContext<any> {
	constructor(host: UmbControllerHostElement) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_PICKER_MODAL);
	}
}
