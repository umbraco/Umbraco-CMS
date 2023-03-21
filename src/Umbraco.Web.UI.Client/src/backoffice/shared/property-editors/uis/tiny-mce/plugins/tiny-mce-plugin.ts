import { Editor } from "tinymce";
import { DataTypePropertyPresentationModel } from "@umbraco-cms/backend-api";
import { UmbElementMixinInterface } from "@umbraco-cms/element";

export interface TinyMcePluginArguments {
    host: UmbElementMixinInterface;
	editor: Editor;
	configuration?: Array<DataTypePropertyPresentationModel>;
}

export class TinyMcePluginBase {
    host: UmbElementMixinInterface;
    editor: Editor;
    configuration?: Array<DataTypePropertyPresentationModel>;

    constructor(arg: TinyMcePluginArguments) {
        this.host = arg.host;
        this.editor = arg.editor;
        this.configuration = arg.configuration;
    }
}