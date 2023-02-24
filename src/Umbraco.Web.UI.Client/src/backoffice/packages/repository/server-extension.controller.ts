import { Subject, takeUntil } from 'rxjs';
import { UmbPackageRepository } from './package.repository';
import { UmbController, UmbControllerHostInterface } from '@umbraco-cms/controller';
import { isManifestJSType, UmbExtensionRegistry } from '@umbraco-cms/extensions-api';

export class UmbServerExtensionController extends UmbController {
	#unobserve = new Subject<void>();
	#repository: UmbPackageRepository;

	constructor(host: UmbControllerHostInterface, private readonly extensionRegistry: UmbExtensionRegistry) {
		super(host, UmbServerExtensionController.name);

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
					/**
					 * Crude check to see if extension is of type "js" since it is safe to assume we do not
					 * need to load any other types of extensions in the backoffice (we need a js file to load)
					 */
					if (isManifestJSType(extension)) {
						this.extensionRegistry.register(extension);
					}
				});
			});
	}
}
