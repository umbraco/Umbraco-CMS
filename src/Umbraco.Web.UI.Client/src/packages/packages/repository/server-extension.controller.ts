import { Subject, takeUntil } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbPackageRepository } from './package.repository.js';
import { UmbController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbExtensionInitializer extends UmbController {
	#host: UmbControllerHostElement;
	#extensionRegistry: UmbBackofficeExtensionRegistry;
	#unobserve = new Subject<void>();
	#repository: UmbPackageRepository;
	#localPackages: Array<Promise<any>>;

	constructor(
		host: UmbControllerHostElement,
		extensionRegistry: UmbBackofficeExtensionRegistry,
		localPackages: Array<Promise<any>>
	) {
		super(host, UmbExtensionInitializer.name);
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;
		this.#repository = new UmbPackageRepository(host);
		this.#localPackages = localPackages;
	}

	hostConnected(): void {
		this.#loadLocalPackages();
		this.#loadServerPackages();
	}

	hostDisconnected(): void {
		this.#unobserve.next();
		this.#unobserve.complete();
	}

	async #loadLocalPackages() {
		this.#localPackages.forEach(async (packageImport) => {
			const packageModule = await packageImport;
			this.#extensionRegistry.registerMany(packageModule.extensions);
		});
	}

	async #loadServerPackages() {
		const extensions$ = await this.#repository.extensions();

		extensions$
			.pipe(
				// If the app breaks then stop the request
				takeUntil(this.#unobserve)
			)
			.subscribe((extensions) => this.#extensionRegistry.registerMany(extensions));
	}
}
