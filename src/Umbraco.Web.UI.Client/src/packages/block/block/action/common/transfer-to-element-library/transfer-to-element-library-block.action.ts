import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import { UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbTransferToElementLibraryBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#localize = new UmbLocalizationController(this);

	override async execute() {
		// the block is read from the owner's stored data, so anything still only on screen would be left
		// behind. This resolves past a block workspace to the context that persists to the server, which is
		// also the only dirty signal available at any nesting depth.
		const workspaceContext = await this.getContext(UMB_ENTITY_DETAIL_WORKSPACE_CONTEXT).catch(() => undefined);
		if (workspaceContext?.getHasUnpersistedChanges() !== false) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT).catch(() => undefined);
			notificationContext?.peek('warning', {
				data: { message: this.#localize.term('blockEditor_transferToElementLibraryUnsavedText') },
			});
			return;
		}

		const context = await this.getContext(UMB_BLOCK_ENTRY_CONTEXT);
		await context?.requestTransferToExternalContent();
	}
}

export { UmbTransferToElementLibraryBlockAction as api };
