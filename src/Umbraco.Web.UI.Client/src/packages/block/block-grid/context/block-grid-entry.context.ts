import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context.js';
import {
	UmbBlockContext,
	type UmbBlockGridTypeModel,
	type UmbBlockGridLayoutModel,
	type UmbBlockGridLayoutAreaItemModel,
} from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
export class UmbBlockGridEntryContext extends UmbBlockContext<
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT,
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE,
	UmbBlockGridTypeModel,
	UmbBlockGridLayoutModel
> {
	#areas = new UmbArrayState<UmbBlockGridLayoutAreaItemModel>([], (x) => x.key);
	areas = this.#areas.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT);

		this.observe(this.layout, (layout) => {
			this.#areas.setValue(layout?.areas ?? []);
		});
	}

	_gotManager() {
		super._gotManager();
		if (this._manager) {
			/*this.observe(
				this._manager.inlineEditingMode,
				(inlineEditingMode) => {
					this.#inlineEditingMode.setValue(inlineEditingMode);
				},
				'observeInlineEditingMode',
			);*/
		} else {
			//this.removeControllerByAlias('observeInlineEditingMode');
		}
	}
}
