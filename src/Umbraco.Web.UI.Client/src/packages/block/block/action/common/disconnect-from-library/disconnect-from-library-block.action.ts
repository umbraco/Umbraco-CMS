import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';

/**
 * Block action that disconnects a block from its Library Element reference,
 * copying the element content into local contentData via the entry context's
 * `requestDisconnectFromLibrary()` flow.
 */
export class UmbDisconnectFromLibraryBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	override async execute() {
		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		await context?.requestDisconnectFromLibrary();
	}
}

export { UmbDisconnectFromLibraryBlockAction as api };
