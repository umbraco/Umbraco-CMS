import { UMB_STATIC_FILE_PICKER_MODAL } from '../../modals/index.js';
import { UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import type { UmbStaticFilePickerModalData, UmbStaticFilePickerModalValue } from '../../modals/index.js';
import type { UmbStaticFileItemModel } from '../../types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbStaticFilePickerInputContext extends UmbPickerInputContext<
	UmbStaticFileItemModel,
	UmbStaticFileItemModel,
	UmbStaticFilePickerModalData,
	UmbStaticFilePickerModalValue
> {
	#serializer = new UmbServerFilePathUniqueSerializer();

	constructor(host: UmbControllerHost) {
		super(host, UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);
	}

	protected override async _requestItemName(unique: string): Promise<string> {
		// If item doesn't exist, use the file path as the name
		const item = this.getSelectedItemByUnique(unique);
		return item?.name ?? this.#serializer.toServerPath(unique) ?? unique;
	}
}
