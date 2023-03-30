import { Editor } from "tinymce";
import { TinyMcePluginArguments } from "./tinymce-plugin.model";
import { DataTypePropertyPresentationModel } from "@umbraco-cms/backoffice/backend-api";
import { UmbElementMixinInterface } from "@umbraco-cms/backoffice/element";

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