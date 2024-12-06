import { UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { UMB_SCRIPT_PICKER_MODAL } from '../../modals/script-picker-modal.token.js';
import type { UmbScriptItemModel, UmbScriptTreeItemModel } from '../../index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbScriptPickerInputContext extends UmbPickerInputContext<UmbScriptItemModel, UmbScriptTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_SCRIPT_ITEM_REPOSITORY_ALIAS, UMB_SCRIPT_PICKER_MODAL);
	}
}

/** @deprecated Use `UmbScriptPickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbScriptPickerInputContext as UmbScriptPickerContext };
