import type { ClassConstructor, ManifestApi } from '../types.js';

export function isManifestClassConstructorType<ApiType>(manifest: unknown): manifest is ManifestApiWithClassConstructor<ApiType> {
	return typeof manifest === 'object' && manifest !== null && (manifest as ManifestApi).api !== undefined;
}

export interface ManifestApiWithClassConstructor<T = unknown> extends ManifestApi<T> {
	api: ClassConstructor<T>;
}
