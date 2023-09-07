import type { UmbInputTinyMceElement } from '@umbraco-cms/backoffice/components';
import type { tinymce } from '@umbraco-cms/backoffice/external/tinymce';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export class UmbTinyMcePluginBase {
	host: UmbInputTinyMceElement;
	editor: tinymce.Editor;
	configuration?: UmbPropertyEditorConfigCollection;

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
