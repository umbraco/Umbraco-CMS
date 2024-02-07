import { UMB_BLOCK_LIST_MANAGER_CONTEXT } from './block-list-manager.context.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
export class UmbBlockListEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT,
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT.TYPE
> {
	#inlineEditingMode = new UmbBooleanState(undefined);
	inlineEditingMode = this.#inlineEditingMode.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_LIST_MANAGER_CONTEXT);
	}

	_gotManager() {
		super._gotManager();
		if (this._manager) {
			this.observe(
				this._manager.inlineEditingMode,
				(inlineEditingMode) => {
					this.#inlineEditingMode.setValue(inlineEditingMode);
				},
				'observeInlineEditingMode',
			);
		} else {
			this.removeControllerByAlias('observeInlineEditingMode');
		}
	}
}
