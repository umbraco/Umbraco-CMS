import { UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_STATIC_FILE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbStaticFilePickerContext extends UmbPickerInputContext<DocumentTreeItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);
	}
}
