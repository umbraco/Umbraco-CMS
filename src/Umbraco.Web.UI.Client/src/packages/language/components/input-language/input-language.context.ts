import type { UmbLanguageItemModel } from '../../types.js';
import { UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_LANGUAGE_PICKER_MODAL } from '../../modals/language-picker/constants.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';

export class UmbLanguagePickerInputContext extends UmbPickerInputContext<UmbLanguageItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_LANGUAGE_ITEM_REPOSITORY_ALIAS, UMB_LANGUAGE_PICKER_MODAL);
	}
}

/** @deprecated Use `UmbLanguagePickerInputContext` instead. This method will be removed in Umbraco 15. */
export { UmbLanguagePickerInputContext as UmbLanguagePickerContext };
