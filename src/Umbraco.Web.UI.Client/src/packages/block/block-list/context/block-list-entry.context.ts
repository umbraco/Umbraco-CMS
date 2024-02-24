import { UMB_BLOCK_LIST_MANAGER_CONTEXT } from './block-list-manager.context.js';
import { UMB_BLOCK_LIST_ENTRIES_CONTEXT } from './block-list-entries.context-token.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
export class UmbBlockListEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT,
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_LIST_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_LIST_ENTRIES_CONTEXT.TYPE
> {
	#inlineEditingMode = new UmbBooleanState(undefined);
	readonly inlineEditingMode = this.#inlineEditingMode.asObservable();
	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart(
		(x) => !!x?.forceHideContentEditorInOverlay,
	);

	readonly showContentEdit = mergeObservables(
		[this.forceHideContentEditorInOverlay, this.inlineEditingMode],
		([forceHide, inlineMode]): boolean => {
			return !forceHide && !inlineMode;
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_LIST_MANAGER_CONTEXT, UMB_BLOCK_LIST_ENTRIES_CONTEXT);
	}

	_gotManager() {
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

	_gotEntries() {}

	_gotContentType() {}
}
