import type { UmbTemporaryFileConfigurationModel } from '../types.js';
import { UmbTemporaryFileConfigServerDataSource } from './config.server.data-source.js';
import { UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT } from './config.store.token.js';
import { UMB_TEMPORARY_FILE_REPOSITORY_ALIAS } from './constants.js';
import { UmbConfigRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type Observable, map } from '@umbraco-cms/backoffice/external/rxjs';

// SVG is rendered natively by browsers but can never appear in the server's imageFileTypes,
// because the imaging pipeline cannot process it (#20574).
const ADDITIONAL_DISPLAYABLE_IMAGE_FILE_TYPES = ['svg'];

export class UmbTemporaryFileConfigRepository extends UmbConfigRepositoryBase<UmbTemporaryFileConfigurationModel> {
	#dataSource = new UmbTemporaryFileConfigServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_TEMPORARY_FILE_CONFIG_STORE_CONTEXT, UMB_TEMPORARY_FILE_REPOSITORY_ALIAS.toString());
	}

	protected override _requestConfig() {
		return this.requestTemporaryFileConfiguration();
	}

	async requestTemporaryFileConfiguration() {
		const { data } = await this.#dataSource.getConfig();
		return data;
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
