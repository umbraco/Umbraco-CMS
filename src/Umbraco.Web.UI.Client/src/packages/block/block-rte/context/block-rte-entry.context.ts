import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from './block-rte-entries.context-token.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
export class UmbBlockRteEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE
> {
	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart(
		(x) => !!x?.forceHideContentEditorInOverlay,
	);

	readonly showContentEdit = this.forceHideContentEditorInOverlay;

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_RTE_MANAGER_CONTEXT, UMB_BLOCK_RTE_ENTRIES_CONTEXT);
	}

	_gotManager() {}

	_gotEntries() {}

	_gotContentType() {}
}
