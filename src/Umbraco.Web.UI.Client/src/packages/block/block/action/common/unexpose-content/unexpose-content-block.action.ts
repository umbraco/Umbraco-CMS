import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';

/**
 * Block action that reverts the expose of the block for the current variant via `context.unexpose()`.
 * Fallback for blocks with no workspace to open, where the workspace action menu item is unreachable.
 */
export class UmbUnexposeContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	override async execute() {
		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		context?.unexpose();
	}
}

export { UmbUnexposeContentBlockAction as api };
