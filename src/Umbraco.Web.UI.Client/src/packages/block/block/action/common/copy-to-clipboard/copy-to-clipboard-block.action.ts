import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Block action that copies the block entry data to the clipboard via `context.copyToClipboard()`.
 */
export class UmbCopyToClipboardBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#blockEntryContext?: typeof UMB_BLOCK_ENTRY_CONTEXT.TYPE;

	constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
		super(host, args);

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.#blockEntryContext = context;
		});
	}

	override async execute() {
		await this.#blockEntryContext?.copyToClipboard();
	}
}

export { UmbCopyToClipboardBlockAction as api };
