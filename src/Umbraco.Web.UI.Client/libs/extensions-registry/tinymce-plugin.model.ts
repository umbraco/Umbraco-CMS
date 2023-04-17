import type { ManifestClass } from "./models";
import { DataTypePropertyPresentationModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbElementMixinInterface } from "@umbraco-cms/backoffice/element";

export interface ManifestTinyMcePlugin extends ManifestClass {
	type: 'tinyMcePlugin';
}

export interface TinyMcePluginArguments {
    host: UmbElementMixinInterface;
	editor: any; // TODO => should be typed, but would require libs taking a dependency on TinyMCE, which is not ideal
	configuration?: Array<DataTypePropertyPresentationModel>;
}