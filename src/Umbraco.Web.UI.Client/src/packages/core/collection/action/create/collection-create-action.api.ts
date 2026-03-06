import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbCollectionActionBase } from '../collection-action-base.js';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbExtensionsApiInitializer, type UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbCollectionCreateOption } from './types.js';

type ManifestType = ManifestEntityCreateOptionAction;
export class UmbCreateCollectionActionApi extends UmbCollectionActionBase {
	#entityContext?: typeof UMB_ENTITY_CONTEXT.TYPE;

	#options = new UmbArrayState<UmbCollectionCreateOption>([], (x) => x.alias);
	public readonly options = this.#options.asObservable();

	#multipleOptions = new UmbBooleanState(false);
	public readonly multipleOptions = this.#multipleOptions.asObservable();

	#controllers = new Map<string, UmbExtensionApiInitializer<ManifestType>>();

	constructor(host: UmbControllerHost) {
		super(host);
		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.#entityContext = context;
			this.#initApi();
		});
	}

	#initApi() {
		if (!this.#entityContext) return;

		const entityType = this.#entityContext.getEntityType();
		if (!entityType) throw new Error('No entity type found');

		const unique = this.#entityContext.getUnique();
		if (unique === undefined) throw new Error('No unique found');

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestType) => {
				return [{ entityType, unique, meta: manifest.meta }];
			},
			(manifest: ManifestType) => manifest.forEntityTypes.includes(entityType),
			async (controllers) => {
				const apiControllers = controllers as unknown as Array<UmbExtensionApiInitializer<ManifestType>>;

				this.#controllers.clear();
				const options: Array<UmbCollectionCreateOption> = [];

				for (const controller of apiControllers) {
					const manifest = controller.manifest;
					if (!manifest) continue;

					this.#controllers.set(manifest.alias, controller);

					options.push({
						alias: manifest.alias,
						label: manifest.meta.label ?? manifest.name,
						icon: manifest.meta.icon,
						href: await controller.api?.getHref(),
					});
				}

				this.#options.setValue(options);
				this.#multipleOptions.setValue(options.length > 1);
			},
		);
	}

	async executeByAlias(alias: string) {
		const controller = this.#controllers.get(alias);
		if (!controller?.api) throw new Error('No API found');
		await controller.api.execute();
	}
	async execute() {}
}

export { UmbCreateCollectionActionApi as api };
