import type { ManifestBase } from "./models";
import type { ClassConstructor } from "@umbraco-cms/models";
import { TinyMcePluginBase } from "src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-plugin";

export interface ManifestTinyMcePlugin extends ManifestBase {
	type: 'tinyMcePlugin';
	meta: MetaTinyMcePlugin;
}

export interface MetaTinyMcePlugin {
	api: ClassConstructor<TinyMcePluginBase>;
	args?: any[];
}
