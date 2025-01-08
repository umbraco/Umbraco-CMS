import type { ManifestBase } from '../types/index.js';
import type { UmbApi } from './api.interface.js';

/**
 * Interface for APIs of a Extension.
 */
export interface UmbExtensionApi<ManifestType extends ManifestBase = ManifestBase> extends UmbApi {
	manifest?: ManifestType;
}
