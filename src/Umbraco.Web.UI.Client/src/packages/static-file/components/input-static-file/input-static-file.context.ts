import { UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS } from '../../tree/index.js';
import { UMB_STATIC_FILE_PICKER_MODAL } from '../../modals/index.js';
import type { UmbStaticFileItemModel } from '../../repository/item/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbStaticFilePickerContext extends UmbPickerInputContext<UmbStaticFileItemModel> {
	constructor(host: UmbControllerHostElement) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS, UMB_STATIC_FILE_PICKER_MODAL);
	}
}
