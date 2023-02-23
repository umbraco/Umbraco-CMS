import { Subject, takeUntil } from 'rxjs';
import { UmbPackageRepository } from './package.repository';
import { UmbController, UmbControllerHostInterface } from '@umbraco-cms/controller';
import type { UmbExtensionRegistry } from '@umbraco-cms/extensions-api';

export class UmbServerPackageController extends UmbController {
	#unobserve = new Subject<void>();
	#repository: UmbPackageRepository;

	constructor(host: UmbControllerHostInterface, private readonly extensionRegistry: UmbExtensionRegistry) {
		super(host, UmbServerPackageController.name);

		this.#repository = new UmbPackageRepository(host);
	}

	hostConnected(): void {
		this.#loadPackages();
	}

	hostDisconnected(): void {
		this.#unobserve.next();
		this.#unobserve.complete();
	}

	async #loadPackages() {
		const package$ = await this.#repository.rootItems();

		package$.pipe(takeUntil(this.#unobserve)).subscribe((packages) => {
			// Go through packages and register their extensions
			packages.forEach((p) => {
				const { extensions } = p;
				if (extensions?.length) {
					extensions.forEach((extension: any) => {
						this.extensionRegistry.register(extension);
					});
				}
			});
		});
	}
}
