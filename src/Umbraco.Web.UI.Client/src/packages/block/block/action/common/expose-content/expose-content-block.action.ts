import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';

/**
 * Block action that exposes (creates) the block for the current variant via `context.expose()`.
 * Shown as an alternative to the Edit Content button when the block has not yet been exposed.
 */
export class UmbExposeContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	override async execute() {
		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		context?.expose();
	}
}

export { UmbExposeContentBlockAction as api };
