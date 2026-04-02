import { UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbStylesheetItemModel } from '../../types.js';
import type { UmbStylesheetTreeItemModel } from '../../tree/types.js';
import { UMB_STYLESHEET_PICKER_MODAL } from './stylesheet-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStylesheetPickerInputContext extends UmbPickerInputContext<
	UmbStylesheetItemModel,
	UmbStylesheetTreeItemModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_STYLESHEET_ITEM_REPOSITORY_ALIAS, UMB_STYLESHEET_PICKER_MODAL);
	}
}
