import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeActionBase } from '../tree-action-base.js';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbTreeCreateOption } from './types.js';

type ManifestType = ManifestEntityCreateOptionAction;

export class UmbTreeCreateActionApi extends UmbTreeActionBase {
	#options = new UmbArrayState<UmbTreeCreateOption>([], (x) => x.alias);
	public readonly options = this.#options.asObservable();

	#multipleOptions = new UmbBooleanState(false);
	public readonly multipleOptions = this.#multipleOptions.asObservable();

	#controllers = new Map<string, UmbExtensionApiInitializer<ManifestType>>();
	#extensionsInitializer?: UmbExtensionsApiInitializer<ManifestType>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;

			this.observe(
				combineLatest([context.entityType, context.unique]),
				([entityType, unique]) => {
					if (!entityType || unique === undefined) {
						this.#options.setValue([]);
						this.#multipleOptions.setValue(false);
						return;
					}

					this.#extensionsInitializer?.destroy();
					this.#extensionsInitializer = new UmbExtensionsApiInitializer(
						this,
						umbExtensionsRegistry,
						'entityCreateOptionAction',
						(manifest: ManifestType) => [{ entityType, unique, meta: manifest.meta }],
						(manifest: ManifestType) => manifest.forEntityTypes.includes(entityType),
						async (controllers) => {
							const apiControllers = controllers as unknown as Array<UmbExtensionApiInitializer<ManifestType>>;
							this.#controllers.clear();
							const options: Array<UmbTreeCreateOption> = [];

							for (const controller of apiControllers) {
								const manifest = controller.manifest;
								if (!manifest) continue;
								this.#controllers.set(manifest.alias, controller);
								options.push({
									alias: manifest.alias,
									label: manifest.meta.label ?? manifest.name,
									icon: manifest.meta.icon,
									href: await controller.api?.getHref(),
									additionalOptions: manifest.meta.additionalOptions,
								});
							}

							this.#options.setValue(options);
							this.#multipleOptions.setValue(options.length > 1);
						},
					) as unknown as UmbExtensionsApiInitializer<ManifestType>;
				},
				'umbEntityContextObserver',
			);
		});
	}

	async executeByAlias(alias: string) {
		const controller = this.#controllers.get(alias);
		if (!controller?.api) throw new Error('No API found');
		await controller.api.execute();
	}

	async execute() {}
}

export { UmbTreeCreateActionApi as api };
