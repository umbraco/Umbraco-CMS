import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UmbRequestReloadStructureForEntityEvent } from '../../request-reload-structure-for-entity.event.js';
import type { MetaEntityActionDeleteKind } from './types.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

type UmbLocalizeStringArgsMap = Record<string, Array<string>>;

export class UmbDeleteEntityAction extends UmbEntityActionBase<MetaEntityActionDeleteKind> {
	// TODO: make base type for item and detail models
	#localize = new UmbLocalizationController(this);

	override async execute() {
		if (!this.args.unique) throw new Error('Cannot delete an item without a unique identifier.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this.args.meta.itemRepositoryAlias,
		);

		const { data } = await itemRepository.requestItems([this.args.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		const headline = this.args.meta.confirm?.headline ?? '#actions_delete';
		const content = this.args.meta.confirm?.content ?? '#defaultdialogs_confirmdelete';
		const keysFromString = this.#localize.getKeysFromString(content);
		const argsMap: UmbLocalizeStringArgsMap = keysFromString.reduce((acc: UmbLocalizeStringArgsMap, key) => {
			acc[key] = [item.name];
			return acc;
		}, {});

		// TODO: handle items with variants
		await umbConfirmModal(this._host, {
			headline: this.#localize.string(headline),
			content: this.#localize.string(content, argsMap),
			color: 'danger',
			confirmLabel: 'Delete',
		});

		const detailRepository = await createExtensionApiByAlias<UmbDetailRepository<any>>(
			this,
			this.args.meta.detailRepositoryAlias,
		);
		await detailRepository.delete(this.args.unique);

		const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		const event = new UmbRequestReloadStructureForEntityEvent({
			unique: this.args.unique,
			entityType: this.args.entityType,
		});

		actionEventContext.dispatchEvent(event);
	}
}
export default UmbDeleteEntityAction;
