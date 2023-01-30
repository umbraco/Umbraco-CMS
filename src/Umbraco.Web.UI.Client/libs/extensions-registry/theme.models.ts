import type { ManifestWithLoader } from "./models";


// TODO: make or find type for JS Module with default export: Would be nice to support css file directly.
export interface ManifestTheme extends ManifestWithLoader<any> {
	type: 'theme';
}
