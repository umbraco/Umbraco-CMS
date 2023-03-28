import type { ManifestWithMeta } from "./models";

export interface ManifestTinyMcePlugin extends ManifestWithMeta {
	type: 'tinyMcePlugin';
	meta: MetaTinyMcePlugin;
}

export interface MetaTinyMcePlugin {
	exportName: string;
	js: string;
}
