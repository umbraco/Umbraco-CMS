import { UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS } from '../../repository/item/index.js';
import { UMB_PARTIAL_VIEW_PICKER_MODAL } from '../../partial-view-picker/index.js';
import type { UmbPartialViewItemModel } from '../../types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPartialViewPickerContext extends UmbPickerInputContext<UmbPartialViewItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS, UMB_PARTIAL_VIEW_PICKER_MODAL);
	}
}
