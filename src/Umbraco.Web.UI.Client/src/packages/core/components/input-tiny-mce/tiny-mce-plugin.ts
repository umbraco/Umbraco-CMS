import type { UmbDataTypeConfigCollection, UmbInputTinyMceElement } from '@umbraco-cms/backoffice/components';
import type { tinymce } from '@umbraco-cms/backoffice/external/tinymce';

export class UmbTinyMcePluginBase {
	host: UmbInputTinyMceElement;
	editor: tinymce.Editor;
	configuration?: UmbDataTypeConfigCollection;

	constructor(arg: TinyMcePluginArguments) {
		this.host = arg.host;
		this.editor = arg.editor;
		this.configuration = arg.host.configuration;
	}
}

export interface TinyMcePluginArguments {
	host: UmbInputTinyMceElement;
	editor: tinymce.Editor;
}
