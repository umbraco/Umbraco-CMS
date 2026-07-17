import type { UmbTemporaryFileConfigurationModel } from '../types.js';
import { UmbTemporaryFileConfigServerDataSource } from './config.server.data-source.js';
import { UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT } from './config.store.token.js';
import { UMB_TEMPORARY_FILE_REPOSITORY_ALIAS } from './constants.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { from, map, switchMap } from '@umbraco-cms/backoffice/external/rxjs';

// SVG is rendered natively by browsers but can never appear in the server's imageFileTypes,
// because the imaging pipeline cannot process it (#20574).
const ADDITIONAL_DISPLAYABLE_IMAGE_FILE_TYPES = ['svg'];

export class UmbTemporaryFileConfigRepository extends UmbRepositoryBase implements UmbApi {
	/**
	 * Promise that resolves when the repository has been initialized, i.e. when the configuration has been fetched from the server.
	 * Awaiting this is no longer required before calling all(), part() or displayableImageFileTypes() — they defer internally.
	 */
	initialized;

	#dataStore?: typeof UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT.TYPE;
	#dataSource = new UmbTemporaryFileConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPORARY_FILE_REPOSITORY_ALIAS.toString());
		this.initialized = new Promise<void>((resolve) => {
			this.consumeContext(UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT, async (store) => {
				if (store) {
					this.#dataStore = store;
					await this.#init();
					resolve();
				}
			});
		});
	}

	async #init() {
		// Check if the store already has data
		if (this.#dataStore?.getState()) {
			return;
		}

		const temporaryFileConfig = await this.requestTemporaryFileConfiguration();

		if (temporaryFileConfig) {
			this.#dataStore?.update(temporaryFileConfig);
		}
	}

	async requestTemporaryFileConfiguration() {
		const { data } = await this.#dataSource.getConfig();
		return data;
	}

	/**
	 * Subscribe to the entire configuration.
	 * @returns {Observable<UmbTemporaryFileConfigurationModel>}
	 */
	all() {
		return from(this.initialized).pipe(switchMap(() => this.#dataStore!.all()));
	}

	/**
	 * Subscribe to a part of the configuration.
	 * @param part
	 * @returns {Observable<UmbTemporaryFileConfigurationModel[Part]>}
	 */
	part<Part extends keyof UmbTemporaryFileConfigurationModel>(
		part: Part,
	): Observable<UmbTemporaryFileConfigurationModel[Part]> {
		return from(this.initialized).pipe(switchMap(() => this.#dataStore!.part(part)));
	}

	/**
	 * Subscribe to the image file types that can be displayed directly by the browser.
	 * This is the configured `imageFileTypes` plus formats browsers render natively but the server's
	 * imaging pipeline cannot process (e.g. svg), so it is suited for display-only consumers such as
	 * image pickers and previews — not for upload or cropping contexts, where the server-configured
	 * `imageFileTypes` applies as-is.
	 * @returns {Observable<Array<string>>} The file extensions, lowercased and without leading dots.
	 */
	displayableImageFileTypes(): Observable<Array<string>> {
		return this.part('imageFileTypes').pipe(
			map((fileTypes) => [
				...new Set([
					...(fileTypes ?? []).map((type) => type.toLowerCase()),
					...ADDITIONAL_DISPLAYABLE_IMAGE_FILE_TYPES,
				]),
			]),
		);
	}
}

export default UmbTemporaryFileConfigRepository;
