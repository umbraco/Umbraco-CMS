import type { UmbInputTinyMceElement } from './input-tiny-mce.element.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export class UmbTinyMcePluginBase extends UmbControllerBase implements UmbApi {
	editor: Editor;
	configuration?: UmbPropertyEditorConfigCollection;

	constructor(arg: TinyMcePluginArguments) {
		super(arg.host);
		this.editor = arg.editor;
		this.configuration = arg.host.configuration;
	}
}

export type TinyMcePluginArguments = {
	host: UmbInputTinyMceElement;
	editor: Editor;
};
