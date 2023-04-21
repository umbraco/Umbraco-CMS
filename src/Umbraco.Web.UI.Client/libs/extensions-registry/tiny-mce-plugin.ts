import { TinyMcePluginArguments } from "./tinymce-plugin.model";
import { DataTypePropertyPresentationModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbElementMixinInterface } from "@umbraco-cms/backoffice/element";

// TODO => editor property should be typed, but would require libs taking a dependency on TinyMCE, which is not ideal
export class UmbTinyMcePluginBase {
    host: UmbElementMixinInterface;
    editor: any;
    configuration?: Array<DataTypePropertyPresentationModel>;

    constructor(arg: TinyMcePluginArguments) {
        this.host = arg.host;
        this.editor = arg.editor;
        this.configuration = arg.configuration;
    }
}