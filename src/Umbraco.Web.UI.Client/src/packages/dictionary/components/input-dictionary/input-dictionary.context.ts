import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_DICTIONARY_PICKER_MODAL } from '../../modals/dictionary-picker-modal.token.js';
import type { UmbDictionaryItemModel } from '../../repository/index.js';
import type { UmbDictionaryTreeItemModel } from '../../tree/types.js';

export class UmbDictionaryPickerContext extends UmbPickerInputContext<
	UmbDictionaryItemModel,
	UmbDictionaryTreeItemModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_DICTIONARY_ITEM_REPOSITORY_ALIAS, UMB_DICTIONARY_PICKER_MODAL);
	}
}
