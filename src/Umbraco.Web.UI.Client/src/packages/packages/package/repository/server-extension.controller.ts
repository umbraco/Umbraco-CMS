import { UmbPackageRepository } from './package.repository.js';
import { type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbExtensionInitializer extends UmbBaseController {
	#extensionRegistry: UmbBackofficeExtensionRegistry;
	#repository: UmbPackageRepository;
	#localPackages: Array<Promise<any>>;

	constructor(
		host: UmbControllerHostElement,
		extensionRegistry: UmbBackofficeExtensionRegistry,
		localPackages: Array<Promise<any>>,
	) {
		super(host, UmbExtensionInitializer.name);

		this.#extensionRegistry = extensionRegistry;
		this.#repository = new UmbPackageRepository(host);
		this.#localPackages = localPackages;
	}

	hostConnected(): void {
		this.#loadLocalPackages();
		this.#loadServerPackages();
	}

	hostDisconnected(): void {
		this.removeControllerByAlias('_observeExtensions');
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
