import { UmbPickerInputContext } from 'libs/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UMB_LANGUAGE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { LanguageItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbLanguagePickerContext extends UmbPickerInputContext<LanguageItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.Language', UMB_LANGUAGE_PICKER_MODAL, (item) => item.isoCode);
	}
}
