import type { UmbDataMapper } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi, type ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbDataMapperResolver extends UmbControllerBase {
	#apiCache = new Map<string, UmbDataMapper>();

	async resolve($type: string): Promise<UmbDataMapper | undefined> {
		if (!$type) {
			throw new Error('data is required');
		}

		const manifest = this.#getManifestWithBestFit($type);

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

	#getManifestWithBestFit($type: string) {
		const supportedManifests = this.#getSupportedManifests($type);

		if (!supportedManifests.length) {
			return undefined;
		}

		// Pick the manifest with the highest priority
		return supportedManifests.sort((a: ManifestBase, b: ManifestBase): number => (b.weight || 0) - (a.weight || 0))[0];
	}

	#getSupportedManifests($type: string) {
		const supportedManifests = umbExtensionsRegistry.getByTypeAndFilter('$typeDataMapper', (manifest) => {
			return manifest.from$type === $type;
		});

		return supportedManifests;
	}
}
