import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';

export class UmbDisconnectFromElementLibraryBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	override async execute() {
		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		await context?.requestDisconnectFromExternalContent();
	}
}

export { UmbDisconnectFromElementLibraryBlockAction as api };
