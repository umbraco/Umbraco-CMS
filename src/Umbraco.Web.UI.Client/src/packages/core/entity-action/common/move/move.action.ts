import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbEntityActionArgs } from '../../types.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbItemRepository, UmbMoveRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry, type MetaEntityActionMoveKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

export class UmbMoveEntityAction extends UmbEntityActionBase<any> {
	// TODO: make base type for item and detail models
	#itemRepository?: UmbItemRepository<any>;
	#moveRepository?: UmbMoveRepository;
	#init: Promise<unknown>;

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<MetaEntityActionMoveKind>) {
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
				this.args.meta.moveRepositoryAlias,
				[this._host],
				(permitted, ctrl) => {
					this.#moveRepository = permitted ? (ctrl.api as UmbMoveRepository) : undefined;
				},
			).asPromise(),
		]);
	}

	async execute() {
		if (!this.args.unique) throw new Error('Unique is not available');
		await this.#init;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, this.args.meta.pickerModalAlias) as any; // TODO: make generic picker interface with selection
		const value = await modalContext.onSubmit();
		if (!value) return;
		await this.#moveRepository!.move(this.args.unique, value.selection[0]);
	}

	destroy(): void {}
}
