import { UMB_BLOCK_SINGLE_MANAGER_CONTEXT } from './block-single-manager.context-token.js';
import { UMB_BLOCK_SINGLE_ENTRIES_CONTEXT } from './block-single-entries.context-token.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
export class UmbBlockSingleEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_SINGLE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_SINGLE_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_SINGLE_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_SINGLE_ENTRIES_CONTEXT.TYPE
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
		super(host, UMB_BLOCK_SINGLE_MANAGER_CONTEXT, UMB_BLOCK_SINGLE_ENTRIES_CONTEXT);
	}

	protected override _gotManager() {
		this.observe(
			this._manager?.inlineEditingMode,
			(inlineEditingMode) => {
				this.#inlineEditingMode.setValue(inlineEditingMode);
			},
			'observeInlineEditingMode',
		);
	}

	protected override _gotEntries() {}

	protected override _gotContentType() {}
}
