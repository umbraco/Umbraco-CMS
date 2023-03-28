import { TinyMcePluginArguments, TinyMcePluginBase } from "../../src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-plugin";
import type { ManifestWithMeta } from "./models";
import type { ClassConstructor } from "@umbraco-cms/backoffice/models";

export interface ManifestTinyMcePlugin extends ManifestWithMeta {
	type: 'tinyMcePlugin';
	meta: MetaTinyMcePlugin;
}

export interface MetaTinyMcePlugin {
	api: ClassConstructor<TinyMcePluginBase>;
	args?: TinyMcePluginArguments;
}
