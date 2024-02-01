import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_LANGUAGE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbLanguagePickerContext extends UmbPickerInputContext<UmbLanguageItemModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_LANGUAGE_ITEM_REPOSITORY, UMB_LANGUAGE_PICKER_MODAL);
	}
}
