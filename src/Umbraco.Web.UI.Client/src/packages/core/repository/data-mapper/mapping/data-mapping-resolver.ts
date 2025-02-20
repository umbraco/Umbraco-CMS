import type { UmbDataMapping } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi, type ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDataMappingResolver extends UmbControllerBase {
	#apiCache = new Map<string, UmbDataMapping>();

	async resolve(forDataSource: string, forDataModel: string): Promise<UmbDataMapping | undefined> {
		if (!forDataSource) {
			throw new Error('data source identifier is required');
		}

		if (!forDataModel) {
			throw new Error('data identifier is required');
		}

		const manifest = this.#getManifestWithBestFit(forDataSource, forDataModel);

		if (!manifest) {
			return undefined;
		}

		// Check the cache before creating a new instance
		if (this.#apiCache.has(manifest.alias)) {
			return this.#apiCache.get(manifest.alias)!;
		}

		const dataMapping = await createExtensionApi<UmbDataMapping>(this, manifest);

		if (!dataMapping) {
			return undefined;
		}

		if (!dataMapping.map) {
			throw new Error('Data Mapping does not have a map method.');
		}

		// Cache the api instance for future use
		this.#apiCache.set(manifest.alias, dataMapping);

		return dataMapping;
	}

	#getManifestWithBestFit(forDataSource: string, forDataModel: string) {
		const supportedManifests = this.#getSupportedManifests(forDataSource, forDataModel);

		if (!supportedManifests.length) {
			return undefined;
		}

		// Pick the manifest with the highest priority
		// TODO: This should have been handled in the extension registry, but until then we do it here: [NL]
		return supportedManifests.sort((a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0))[0];
	}

	#getSupportedManifests(forDataSource: string, forDataModel: string) {
		const supportedManifests = umbExtensionsRegistry.getByTypeAndFilter('dataMapping', (manifest) => {
			return manifest.forDataSource === forDataSource && manifest.forDataModel === forDataModel;
		});

		return supportedManifests;
	}
}
