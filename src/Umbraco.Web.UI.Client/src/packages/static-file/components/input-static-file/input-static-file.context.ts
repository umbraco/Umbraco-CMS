import { UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS } from '../../repository/item/manifests.js';
import {
	UMB_STATIC_FILE_PICKER_MODAL,
	type UmbStaticFilePickerModalData,
	type UmbStaticFilePickerModalValue,
} from '../../modals/index.js';
import type { UmbStaticFileItemModel } from '../../repository/item/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStaticFilePickerContext extends UmbPickerInputContext<
	UmbStaticFileItemModel,
	any,
	UmbStaticFilePickerModalData,
	UmbStaticFilePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);
	}
}
