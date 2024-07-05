import { UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS } from '../../repository/item/index.js';
import { UMB_PARTIAL_VIEW_PICKER_MODAL } from '../../partial-view-picker/index.js';
import type { UmbPartialViewItemModel } from '../../types.js';
import type { UmbPartialViewTreeItemModel } from '../../tree/types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';

export class UmbPartialViewPickerContext extends UmbPickerInputContext<
	UmbPartialViewItemModel,
	UmbPartialViewTreeItemModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS, UMB_PARTIAL_VIEW_PICKER_MODAL);
	}
}
