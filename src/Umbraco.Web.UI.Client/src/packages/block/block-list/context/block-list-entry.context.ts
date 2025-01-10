import { UMB_BLOCK_LIST_MANAGER_CONTEXT } from './block-list-manager.context-token.js';
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
	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart((x) =>
		x ? (x.forceHideContentEditorInOverlay ?? false) : undefined,
	);

	readonly showContentEdit = mergeObservables(
		[this._contentStructureHasProperties, this.forceHideContentEditorInOverlay, this.inlineEditingMode],
		([a, b, c]): boolean => {
			return a === true && b === false && c === false;
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_LIST_MANAGER_CONTEXT, UMB_BLOCK_LIST_ENTRIES_CONTEXT);
	}

	_gotManager() {
		this.observe(
			this._manager?.inlineEditingMode,
			(inlineEditingMode) => {
				this.#inlineEditingMode.setValue(inlineEditingMode);
			},
			'observeInlineEditingMode',
		);
	}

	_gotEntries() {}

	_gotContentType() {}
}
