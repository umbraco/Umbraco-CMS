import type { ClassConstructor, ExtensionApi, ManifestApi } from '../types.js';

export function isManifestApiConstructorType<ApiType extends ExtensionApi>(manifest: unknown): manifest is ManifestApiWithClassConstructor<ApiType> {
	return typeof manifest === 'object' && manifest !== null && (manifest as ManifestApi).api !== undefined;
}

export interface ManifestApiWithClassConstructor<T extends ExtensionApi = ExtensionApi> extends ManifestApi<T> {
	api: ClassConstructor<T>;
}
