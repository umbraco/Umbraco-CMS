import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';

/**
 * Block action that transfers a local block's content into a reusable Library Element
 * via the entry context's `requestTransferToLibrary()` flow.
 */
export class UmbTransferToLibraryBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	override async execute() {
		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		await context?.requestTransferToLibrary();
	}
}

export { UmbTransferToLibraryBlockAction as api };
