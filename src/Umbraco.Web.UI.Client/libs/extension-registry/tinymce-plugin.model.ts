import type { ManifestClass } from "./models";
import { DataTypePropertyPresentationModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbElementMixinInterface } from "@umbraco-cms/backoffice/element";

export interface ManifestTinyMcePlugin extends ManifestClass {
	type: 'tinyMcePlugin';
}

// TODO => editor property should be typed, but would require libs taking a dependency on TinyMCE, which is not ideal
export interface TinyMcePluginArguments {
    host: UmbElementMixinInterface;
	editor: any; 
	configuration?: Array<DataTypePropertyPresentationModel>;
}