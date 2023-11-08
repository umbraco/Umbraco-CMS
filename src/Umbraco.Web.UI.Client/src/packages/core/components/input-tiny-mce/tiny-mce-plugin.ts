import type { UmbInputTinyMceElement } from '@umbraco-cms/backoffice/components';
import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export class UmbTinyMcePluginBase {
	host: UmbInputTinyMceElement;
	editor: Editor;
	configuration?: UmbPropertyEditorConfigCollection;

	constructor(arg: TinyMcePluginArguments) {
		this.host = arg.host;
		this.editor = arg.editor;
		this.configuration = arg.host.configuration;
	}
}

export interface TinyMcePluginArguments {
	host: UmbInputTinyMceElement;
	editor: Editor;
}
