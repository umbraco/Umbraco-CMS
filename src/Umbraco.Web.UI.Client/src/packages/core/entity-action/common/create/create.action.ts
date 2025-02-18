import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbEntityActionArgs } from '../../types.js';
import type { MetaEntityActionCreateKind } from './types.js';
import { UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL } from './modal/constants.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi, UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbCreateEntityAction extends UmbEntityActionBase<MetaEntityActionCreateKind> {
	#hasSingleOption = true;
	#singleActionOptionManifest?: ManifestEntityCreateOptionAction;

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<MetaEntityActionCreateKind>) {
		super(host, args);

		new UmbExtensionsManifestInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(ext) => ext.forEntityTypes.includes(this.args.entityType),
			async (actionOptions) => {
				this.#hasSingleOption = actionOptions.length === 1;
				this.#singleActionOptionManifest = this.#hasSingleOption
					? (actionOptions[0].manifest as unknown as ManifestEntityCreateOptionAction)
					: undefined;
			},
			'umbEntityActionsObserver',
		);
	}

	override async execute() {
		if (this.#hasSingleOption) {
			if (!this.#singleActionOptionManifest) throw new Error('No first action manifest found');

			const api = await createExtensionApi(this, this.#singleActionOptionManifest, [
				{ unique: this.args.unique, entityType: this.args.entityType, meta: this.#singleActionOptionManifest.meta },
			]);

			if (!api) throw new Error(`Could not create api for ${this.#singleActionOptionManifest.alias}`);

			await api.execute();
			return;
		}

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
			},
		});

		await modalContext.onSubmit();
	}
}

export { UmbCreateEntityAction as api };
