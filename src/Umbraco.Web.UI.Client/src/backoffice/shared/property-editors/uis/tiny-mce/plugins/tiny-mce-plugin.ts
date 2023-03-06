import { Editor } from "tinymce";
import { UmbMediaHelper } from "../media-helper.service";
import { DataTypePropertyModel } from "@umbraco-cms/backend-api";
import { UmbModalContext } from "@umbraco-cms/modal";
import type { UserDetails } from "@umbraco-cms/models";

export interface TinyMcePluginArguments {
	editor: Editor;
	modalContext?: UmbModalContext;
	configuration?: Array<DataTypePropertyModel>;
	currentUser?: UserDetails;
	mediaHelper?: UmbMediaHelper;
}

export class TinyMcePluginBase {
    editor!: Editor;
    modalContext?: UmbModalContext;
    currentUser?: UserDetails;
    mediaHelper?: UmbMediaHelper;
    configuration?: Array<DataTypePropertyModel>;

    constructor(arg: TinyMcePluginArguments) {
        this.editor = arg.editor;
        this.modalContext = arg.modalContext;
        this.currentUser = arg.currentUser;
        this.mediaHelper = arg.mediaHelper;
        this.configuration = arg.configuration;
    }
}