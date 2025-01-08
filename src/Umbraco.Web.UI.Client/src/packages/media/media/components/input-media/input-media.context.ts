import { UMB_MEDIA_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_MEDIA_PICKER_MODAL } from '../../modals/index.js';
import type { UmbMediaItemModel } from '../../repository/item/types.js';
import type { UmbMediaPickerModalData, UmbMediaPickerModalValue } from '../../modals/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaPickerInputContext extends UmbPickerInputContext<
	UmbMediaItemModel,
	UmbMediaItemModel,
	UmbMediaPickerModalData<UmbMediaItemModel>,
	UmbMediaPickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_PICKER_MODAL);
	}
}
