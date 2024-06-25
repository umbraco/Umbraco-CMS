import { UmbPackageRepository } from './package.repository.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbExtensionInitializer extends UmbControllerBase {
	#extensionRegistry: UmbBackofficeExtensionRegistry;
	#repository: UmbPackageRepository;
	#localPackages: Array<Promise<any>>;

	constructor(
		host: UmbControllerHost,
		extensionRegistry: UmbBackofficeExtensionRegistry,
		localPackages: Array<Promise<any>>,
	) {
		super(host, UmbExtensionInitializer.name);

		this.#extensionRegistry = extensionRegistry;
		this.#repository = new UmbPackageRepository(host);
		this.#localPackages = localPackages;
	}

	override hostConnected(): void {
		this.#loadLocalPackages();
		this.#loadServerPackages();
	}

	override hostDisconnected(): void {
		this.removeUmbControllerByAlias('_observeExtensions');
	}

	async #loadLocalPackages() {
		this.#localPackages.forEach(async (packageImport) => {
			const packageModule = await packageImport;
			this.#extensionRegistry.registerMany(packageModule.extensions);
		});
	}

	async #loadServerPackages() {
		const extensions = await this.#repository.extensions();

		this.observe(extensions, (extensions) => this.#extensionRegistry.registerMany(extensions), '_observeExtensions');
	}
}
