import type { UmbDataMapper } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi, type ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDataMapperResolver extends UmbControllerBase {
	#apiCache = new Map<string, UmbDataMapper>();

	async resolve(identifier: string): Promise<UmbDataMapper | undefined> {
		if (!identifier) {
			throw new Error('data is required');
		}

		const manifest = this.#getManifestWithBestFit(identifier);

		if (!manifest) {
			return undefined;
		}

		// Check the cache before creating a new instance
		if (this.#apiCache.has(manifest.alias)) {
			return this.#apiCache.get(manifest.alias)!;
		}

		const dataMapper = await createExtensionApi<UmbDataMapper>(this, manifest);

		if (!dataMapper) {
			return undefined;
		}

		if (!dataMapper.map) {
			throw new Error('Data Mapper does not have a map method.');
		}

		// Cache the api instance for future use
		this.#apiCache.set(manifest.alias, dataMapper);

		return dataMapper;
	}

	#getManifestWithBestFit(identifier: string) {
		const supportedManifests = this.#getSupportedManifests(identifier);

		if (!supportedManifests.length) {
			return undefined;
		}

		// Pick the manifest with the highest priority
		return supportedManifests.sort((a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0))[0];
	}

	#getSupportedManifests(identifier: string) {
		const supportedManifests = umbExtensionsRegistry.getByTypeAndFilter('dataMapper', (manifest) => {
			return manifest.identifier === identifier;
		});

		return supportedManifests;
	}
}
