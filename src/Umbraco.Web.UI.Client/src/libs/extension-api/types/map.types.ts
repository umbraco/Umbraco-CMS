import type { ManifestBase } from './manifest-base.interface.js';

type ManifestTypeMapGenerator<ManifestTypes extends ManifestBase> = {
	[Manifest in ManifestTypes as Manifest['type']]: Manifest;
} & {
	[key: string]: ManifestBase;
};

// eslint-disable-next-line @typescript-eslint/naming-convention
export type SpecificManifestTypeOrManifestBase<
	ManifestTypes extends ManifestBase,
	T extends string,
	ManifestTypeMapType = ManifestTypeMapGenerator<ManifestTypes>,
> = T extends keyof ManifestTypeMapType ? ManifestTypeMapType[T] : ManifestBase;
