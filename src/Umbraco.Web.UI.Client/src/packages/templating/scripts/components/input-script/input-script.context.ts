import { UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../../repository/item/index.js';
import { UMB_SCRIPT_PICKER_MODAL } from '../../modals/script-picker-modal.token.js';
import type { UmbScriptItemModel } from '../../index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptPickerContext extends UmbPickerInputContext<UmbScriptItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_SCRIPT_ITEM_REPOSITORY_ALIAS, UMB_SCRIPT_PICKER_MODAL);
	}
}
