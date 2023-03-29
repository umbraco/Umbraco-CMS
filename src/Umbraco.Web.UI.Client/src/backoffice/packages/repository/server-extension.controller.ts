import { Subject, takeUntil } from 'rxjs';
import { UmbPackageRepository } from './package.repository';
import { UmbController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extensions-api';

export class UmbServerExtensionController extends UmbController {
	#host: UmbControllerHostElement;
	#unobserve = new Subject<void>();
	#repository: UmbPackageRepository;

	constructor(host: UmbControllerHostElement, private readonly extensionRegistry: UmbExtensionRegistry) {
		super(host, UmbServerExtensionController.name);
		this.#host = host;
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
		const extensions$ = await this.#repository.extensions();

		extensions$
			.pipe(
				// If the app breaks then stop the request
				takeUntil(this.#unobserve)
			)
			.subscribe((extensions) => {
				extensions.forEach((extension) => {
					this.extensionRegistry.register(extension);
				});
			});
	}
}
