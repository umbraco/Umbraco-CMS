import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityActionBase, UmbRequestReloadStructureForEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { html } from '@umbraco-cms/backoffice/external/lit';

export class UmbUnregisterExtensionEntityAction extends UmbEntityActionBase<unknown> {
	override async execute() {
		if (!this.args.unique) throw new Error('Cannot delete an item without a unique identifier.');

		const extension = umbExtensionsRegistry.getByAlias(this.args.unique);
		if (!extension) throw new Error('Extension not found');

		await umbConfirmModal(this, {
			headline: 'Unregister extension',
			confirmLabel: 'Unregister',
			content: html`<p>Are you sure you want to unregister the extension <strong>${extension.alias}</strong>?</p>`,
			color: 'danger',
		});

		umbExtensionsRegistry.unregister(extension.alias);

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!actionEventContext) {
			throw new Error('Action event context not found');
		}
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}

export { UmbUnregisterExtensionEntityAction as api };
