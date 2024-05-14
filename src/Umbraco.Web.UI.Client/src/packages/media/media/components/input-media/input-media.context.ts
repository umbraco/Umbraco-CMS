import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbMediaTreeItemModel } from '../../tree/index.js';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '../../tree/index.js';
import type {
	UmbMediaTreePickerModalData,
	UmbMediaTreePickerModalValue,
} from '../../tree/media-tree-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaPickerContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaTreeItemModel,
	UmbMediaTreePickerModalData,
	UmbMediaTreePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TREE_PICKER_MODAL);
	}
}
