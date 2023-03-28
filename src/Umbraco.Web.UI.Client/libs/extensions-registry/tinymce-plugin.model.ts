import { TinyMcePluginBase } from "../../src/backoffice/shared/property-editors/uis/tiny-mce/plugins/tiny-mce-plugin";
import type { ManifestClass } from "./models";

export interface ManifestTinyMcePlugin extends ManifestClass<TinyMcePluginBase> {
	type: 'tinyMcePlugin';
}
