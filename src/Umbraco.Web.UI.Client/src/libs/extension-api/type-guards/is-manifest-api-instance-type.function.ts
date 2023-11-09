import type { ClassConstructor, UmbApi, ManifestApi } from '../types.js';

export function isManifestApiConstructorType<ApiType extends UmbApi>(manifest: unknown): manifest is ManifestApiWithClassConstructor<ApiType> {
	return typeof manifest === 'object' && manifest !== null && (manifest as ManifestApi).api !== undefined;
}

export interface ManifestApiWithClassConstructor<T extends UmbApi = UmbApi> extends ManifestApi<T> {
	api: ClassConstructor<T>;
}
