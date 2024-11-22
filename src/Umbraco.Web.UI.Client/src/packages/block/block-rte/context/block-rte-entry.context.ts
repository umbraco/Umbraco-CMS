import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context-token.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from './block-rte-entries.context-token.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { mergeObservables, observeMultiple } from '@umbraco-cms/backoffice/observable-api';
export class UmbBlockRteEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE,
	UmbBlockRteTypeModel,
	UmbBlockRteLayoutModel
> {
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

	_gotManager() {}

	_gotEntries() {
		// Secure displayInline fits configuration:
		this.observe(
			observeMultiple([this.displayInline, this.displayInlineConfig]),
			([displayInline, displayInlineConfig]) => {
				if (displayInlineConfig !== undefined && displayInline !== undefined && displayInlineConfig !== displayInline) {
					const layoutValue = this._layout.getValue();
					if (!layoutValue) return;
					this._layout.setValue({
						...layoutValue,
						displayInline: displayInlineConfig,
					});
				}
			},
			'displayInlineCorrection',
		);
	}

	_gotContentType() {}
}
