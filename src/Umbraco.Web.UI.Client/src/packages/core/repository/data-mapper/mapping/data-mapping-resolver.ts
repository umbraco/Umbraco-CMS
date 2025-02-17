import type { UmbDataMapping } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi, type ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDataMappingResolver extends UmbControllerBase {
	#apiCache = new Map<string, UmbDataMapping>();

	async resolve(dataSourceIdentifier: string, dataModelIdentifier: string): Promise<UmbDataMapping | undefined> {
		if (!dataSourceIdentifier) {
			throw new Error('data source identifier is required');
		}

		if (!dataModelIdentifier) {
			throw new Error('data identifier is required');
		}

		const manifest = this.#getManifestWithBestFit(dataSourceIdentifier, dataModelIdentifier);

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

	#getManifestWithBestFit(dataSourceIdentifier: string, dataModelIdentifier: string) {
		const supportedManifests = this.#getSupportedManifests(dataSourceIdentifier, dataModelIdentifier);

		if (!supportedManifests.length) {
			return undefined;
		}

		// Pick the manifest with the highest priority
		return supportedManifests.sort((a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0))[0];
	}

	#getSupportedManifests(dataSourceIdentifier: string, dataModelIdentifier: string) {
		const supportedManifests = umbExtensionsRegistry.getByTypeAndFilter('dataMapping', (manifest) => {
			return (
				manifest.dataSourceIdentifier === dataSourceIdentifier && manifest.dataModelIdentifier === dataModelIdentifier
			);
		});

		return supportedManifests;
	}
}
