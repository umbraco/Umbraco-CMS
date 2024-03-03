import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbEntityActionArgs } from '../../types.js';
import type { MetaEntityActionDeleteKind } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbDetailRepository, UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbDeleteEntityAction extends UmbEntityActionBase<MetaEntityActionDeleteKind> {
	// TODO: make base type for item and detail models
	#itemRepository?: UmbItemRepository<any>;
	#detailRepository?: UmbDetailRepository<any>;
	#init: Promise<unknown>;

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<MetaEntityActionDeleteKind>) {
		super(host, args);

		// TODO: We should properly look into how we can simplify the one time usage of a extension api, as its a bit of overkill to take conditions/overwrites and observation of extensions into play here: [NL]
		// But since this happens when we execute an action, it does most likely not hurt any users, but it is a bit of a overkill to do this for every action: [NL]
		this.#init = Promise.all([
			new UmbExtensionApiInitializer(
				this._host,
				umbExtensionsRegistry,
				this.args.meta.itemRepositoryAlias,
				[this._host],
				(permitted, ctrl) => {
					this.#itemRepository = permitted ? (ctrl.api as UmbItemRepository<any>) : undefined;
				},
			).asPromise(),

			new UmbExtensionApiInitializer(
				this._host,
				umbExtensionsRegistry,
				this.args.meta.detailRepositoryAlias,
				[this._host],
				(permitted, ctrl) => {
					this.#detailRepository = permitted ? (ctrl.api as UmbDetailRepository<any>) : undefined;
				},
			).asPromise(),
		]);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Cannot delete an item without a unique identifier.');
		await this.#init;

		const { data } = await this.#itemRepository!.requestItems([this.args.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		// TODO: handle items with variants
		await umbConfirmModal(this._host, {
			headline: `Delete`,
			content: `Are you sure you want to delete ${item.name}?`,
			color: 'danger',
			confirmLabel: 'Delete',
		});

		await this.#detailRepository!.delete(this.args.unique);
	}

	destroy(): void {}
}
