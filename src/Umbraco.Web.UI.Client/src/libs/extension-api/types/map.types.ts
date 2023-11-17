import type { ManifestBase } from "./manifest-base.interface.js";

export type ManifestTypeMap<ManifestTypes extends ManifestBase> = {
	[Manifest in ManifestTypes as Manifest['type']]: Manifest;
} & {
	[key: string]: ManifestBase;
};

export type SpecificManifestTypeOrManifestBase<
	ManifestTypes extends ManifestBase,
	T extends keyof ManifestTypeMap<ManifestTypes> | string
> = T extends keyof ManifestTypeMap<ManifestTypes> ? ManifestTypeMap<ManifestTypes>[T] : ManifestBase;
