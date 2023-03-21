import type { ClassConstructor } from "@umbraco-cms/backoffice/models";
import { TinyMcePluginArguments, TinyMcePluginBase } from "../../src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-plugin";
import type { ManifestBase } from "./models";

export interface ManifestTinyMcePlugin extends ManifestBase {
	type: 'tinyMcePlugin';
	meta: MetaTinyMcePlugin;
}

export interface MetaTinyMcePlugin {
	api: ClassConstructor<TinyMcePluginBase>;
	args?: TinyMcePluginArguments;
}
