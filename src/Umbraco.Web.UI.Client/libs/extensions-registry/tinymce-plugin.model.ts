import { Editor } from "tinymce";
import type { ManifestClass } from "./models";
import { DataTypePropertyPresentationModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbElementMixinInterface } from "@umbraco-cms/backoffice/element";

export interface ManifestTinyMcePlugin extends ManifestClass {
	type: 'tinyMcePlugin';
}

export interface TinyMcePluginArguments {
    host: UmbElementMixinInterface;
	editor: Editor;
	configuration?: Array<DataTypePropertyPresentationModel>;
}