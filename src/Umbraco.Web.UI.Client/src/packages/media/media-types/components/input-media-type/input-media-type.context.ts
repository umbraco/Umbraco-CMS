import type { UmbMediaTypeItemModel } from '../../types.js';
import { UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import type {
	UmbMediaTypePickerModalData,
	UmbMediaTypePickerModalValue,
} from '../../tree/media-type-picker-modal.token.js';
import { UMB_MEDIA_TYPE_PICKER_MODAL } from '../../tree/media-type-picker-modal.token.js';
import type { UmbMediaTypeTreeItemModel } from '../../tree/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaTypePickerInputContext extends UmbPickerInputContext<
	UmbMediaTypeItemModel,
	UmbMediaTypeTreeItemModel,
	UmbMediaTypePickerModalData,
	UmbMediaTypePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_PICKER_MODAL);
	}
}

/** @deprecated Use `UmbMediaTypePickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbMediaTypePickerInputContext as UmbMediaTypePickerContext };
