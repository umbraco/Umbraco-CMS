import { UMB_STATIC_FILE_PICKER_MODAL } from '../../modals/index.js';
import type { UmbStaticFilePickerModalData, UmbStaticFilePickerModalValue } from '../../modals/index.js';
import { UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import type { UmbStaticFileItemModel } from '../../types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbServerFilePathUniqueSerializer } from '@umbraco-cms/backoffice/server-file-system';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbStaticFilePickerInputContext extends UmbPickerInputContext<
	UmbStaticFileItemModel,
	UmbStaticFileItemModel,
	UmbStaticFilePickerModalData,
	UmbStaticFilePickerModalValue
> {
	#serializer = new UmbServerFilePathUniqueSerializer();
	#items: Array<UmbStaticFileItemModel> = [];

	constructor(host: UmbControllerHost) {
		super(host, UMB_STATIC_FILE_ITEM_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);

		// Keep track of items for name lookup
		this.observe(this.selectedItems, (items) => {
			this.#items = items;
		});
	}

	override async requestRemoveItem(unique: string) {
		const item = this.#items.find((item) => item.unique === unique);

		// If item doesn't exist, use the file path as the name
		const name = item?.name ?? this.#serializer.toServerPath(unique) ?? unique;

		await umbConfirmModal(this, {
			color: 'danger',
			headline: `#actions_remove?`,
			content: `#defaultdialogs_confirmremove ${name}?`,
			confirmLabel: '#actions_remove',
		});

		this._removeItem(unique);
	}
}
