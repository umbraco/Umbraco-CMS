import { UmbLitElement } from "@umbraco-cms/internal/lit-element";
import { UmbDataTypePropertyCollection } from "@umbraco-cms/backoffice/components";
import { tinymce } from "@umbraco-cms/backoffice/external/tinymce";

export class UmbTinyMcePluginBase {
    host: UmbLitElement;
    editor: tinymce.Editor;
    configuration?: UmbDataTypePropertyCollection;

    constructor(arg: TinyMcePluginArguments) {
        this.host = arg.host;
        this.editor = arg.editor;
        this.configuration = arg.configuration;
    }
}

export interface TinyMcePluginArguments {
    host: UmbLitElement;
	editor: tinymce.Editor; 
	configuration?: UmbDataTypePropertyCollection;
}