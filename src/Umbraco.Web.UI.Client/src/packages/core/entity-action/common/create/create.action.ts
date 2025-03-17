import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbEntityActionArgs } from '../../types.js';
import type { MetaEntityActionCreateKind } from './types.js';
import { UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL } from './modal/constants.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	type UmbExtensionManifestInitializer,
	createExtensionApi,
	UmbExtensionsManifestInitializer,
	type PermittedControllerType,
} from '@umbraco-cms/backoffice/extension-api';
import type {
	ManifestEntityCreateOptionAction,
	UmbEntityCreateOptionAction,
} from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbCreateEntityAction extends UmbEntityActionBase<MetaEntityActionCreateKind> {
	#hasSingleOption = true;
	#optionsInit?: Promise<void>;
	#singleOptionApi?: UmbEntityCreateOptionAction;

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<MetaEntityActionCreateKind>) {
		super(host, args);

		/* This is wrapped in a promise to confirm whether only one option exists and to ensure
		that the API for this option has been created. We both need to wait for any options to
		be returned from the registry and for the API to be created. This is a custom promise implementation,
		because using .asPromise() on the initializer does not wait for the async API creation in the callback.*/
		this.#optionsInit = new Promise((resolve) => {
			new UmbExtensionsManifestInitializer(
				this,
				umbExtensionsRegistry,
				'entityCreateOptionAction',
				(ext) => ext.forEntityTypes.includes(this.args.entityType),
				async (actionOptions) => {
					this.#hasSingleOption = actionOptions.length === 1;
					if (this.#hasSingleOption) {
						await this.#createSingleOptionApi(actionOptions);
						resolve();
					} else {
						resolve();
					}
				},
				'umbEntityActionsObserver',
			);
		});
	}

	override async getHref() {
		await this.#optionsInit;
		const href = await this.#singleOptionApi?.getHref();
		return this.#hasSingleOption && href ? href : undefined;
	}

	override async execute() {
		await this.#optionsInit;
		if (this.#hasSingleOption) {
			await this.#singleOptionApi?.execute();
			return;
		}

		await umbOpenModal(this, UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
			},
		});
	}

	async #createSingleOptionApi(
		createOptions: Array<PermittedControllerType<UmbExtensionManifestInitializer<ManifestEntityCreateOptionAction>>>,
	) {
		const manifest = createOptions[0].manifest;
		if (!manifest) throw new Error('No first action manifest found');

		const api = await createExtensionApi(this, manifest, [
			{ unique: this.args.unique, entityType: this.args.entityType, meta: manifest.meta },
		]);

		if (!api) throw new Error(`Could not create api for ${manifest.alias}`);

		this.#singleOptionApi = api;
	}
}

export { UmbCreateEntityAction as api };
