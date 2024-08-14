import { UMB_PICKER_MODAL_CONTEXT } from './picker-modal.context.token.js';
import { UmbPickerModalSearchManager } from './search/manager/picker-modal-search.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

export class UmbPickerModalContext extends UmbContextBase<UmbPickerModalContext> {
	public readonly selection = new UmbSelectionManager(this);
	public readonly search = new UmbPickerModalSearchManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_MODAL_CONTEXT);

		// TODO: temp hardcoded search provider
		this.search.setConfig({
			providerAlias: 'Umb.SearchProvider.Document',
		});
	}
}
