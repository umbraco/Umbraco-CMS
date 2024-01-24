import { UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_STATIC_FILE_PICKER_MODAL } from '../../modals/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { StaticFileItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbStaticFilePickerContext extends UmbPickerInputContext<StaticFileItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);
	}
}
