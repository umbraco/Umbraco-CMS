import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '../../tree/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaPickerContext extends UmbPickerInputContext<UmbMediaItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TREE_PICKER_MODAL);
	}
}
