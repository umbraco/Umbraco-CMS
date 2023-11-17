import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DICTIONARY_ITEM_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { DictionaryItemItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDictionaryItemPickerContext extends UmbPickerInputContext<DictionaryItemItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.Dictionary', UMB_DICTIONARY_ITEM_PICKER_MODAL);
	}
}
