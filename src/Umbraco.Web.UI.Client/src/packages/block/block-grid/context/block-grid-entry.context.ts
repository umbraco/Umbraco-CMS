import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from './block-grid-entries.context-token.js';
import {
	UmbBlockEntryContext,
	type UmbBlockGridTypeModel,
	type UmbBlockGridLayoutModel,
} from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
export class UmbBlockGridEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT,
	typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_GRID_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_GRID_ENTRIES_CONTEXT.TYPE,
	UmbBlockGridTypeModel,
	UmbBlockGridLayoutModel
> {
	areas = this._layout.asObservablePart((x) => x?.areas ?? []);

	readonly showContentEdit = this._blockType.asObservablePart((x) => !x?.forceHideContentEditorInOverlay);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT, UMB_BLOCK_GRID_ENTRIES_CONTEXT);
	}

	layoutsOfArea(areaKey: string) {
		return this._layout.asObservablePart((x) => x?.areas.find((x) => x.key === areaKey)?.items ?? []);
	}

	setLayoutsOfArea(areaKey: string, layouts: UmbBlockGridLayoutModel[]) {
		const frozenValue = this._layout.value;
		if (!frozenValue) return;
		const areas = appendToFrozenArray(
			frozenValue?.areas,
			{
				key: areaKey,
				items: layouts,
			},
			(x) => x.key,
		);
		this._layout.update({ areas });
	}

	_gotManager() {
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

	_gotEntries() {}
}
