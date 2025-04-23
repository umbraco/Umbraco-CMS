import { UMB_STATIC_FILE_PICKER_MODAL } from '../../modals/index.js';
import type { UmbStaticFilePickerModalData, UmbStaticFilePickerModalValue } from '../../modals/index.js';
import { UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import type { UmbStaticFileItemModel } from '../../types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStaticFilePickerInputContext extends UmbPickerInputContext<
	UmbStaticFileItemModel,
	UmbStaticFileItemModel,
	UmbStaticFilePickerModalData,
	UmbStaticFilePickerModalValue
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);
	}
}
