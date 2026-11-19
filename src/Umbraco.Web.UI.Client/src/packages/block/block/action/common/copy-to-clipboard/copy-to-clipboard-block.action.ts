import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';

/**
 * Block action that copies the block entry data to the clipboard via `context.copyToClipboard()`.
 */
export class UmbCopyToClipboardBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	override async execute() {
		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		await context?.copyToClipboard();
	}
}

export { UmbCopyToClipboardBlockAction as api };
