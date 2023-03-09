import { Subject, takeUntil } from 'rxjs';
import { UmbPackageRepository } from './package.repository';
import { UmbController, UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbExtensionRegistry } from '@umbraco-cms/extensions-api';

export class UmbServerExtensionController extends UmbController {
	#host: UmbControllerHostInterface;
	#unobserve = new Subject<void>();
	#repository: UmbPackageRepository;

	constructor(host: UmbControllerHostInterface, private readonly extensionRegistry: UmbExtensionRegistry) {
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
					this.extensionRegistry.register(extension, this.#host);
				});
			});
	}
}
