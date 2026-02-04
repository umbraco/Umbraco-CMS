import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context-token.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from './block-rte-entries.context-token.js';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBlockRteEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE,
	UmbBlockRteTypeModel,
	UmbBlockRteLayoutModel
> {
	/** @deprecated Use `displayInlineConfig` instead. This field will be removed in Umbraco 19. [LK] */
	readonly displayInline = this._layout.asObservablePart((x) => (x ? (x.displayInline ?? false) : undefined));
	readonly displayInlineConfig = this._blockType.asObservablePart((x) => (x ? (x.displayInline ?? false) : undefined));

	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart((x) =>
		x ? (x.forceHideContentEditorInOverlay ?? false) : undefined,
	);

	readonly showContentEdit = mergeObservables(
		[this._contentStructureHasProperties, this.forceHideContentEditorInOverlay],
		([a, b]): boolean => {
			return a === true && b === false;
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_RTE_MANAGER_CONTEXT, UMB_BLOCK_RTE_ENTRIES_CONTEXT);
	}

	protected override _gotManager() {}

	protected override _gotEntries() {}

	protected override _gotContentType() {}
}
