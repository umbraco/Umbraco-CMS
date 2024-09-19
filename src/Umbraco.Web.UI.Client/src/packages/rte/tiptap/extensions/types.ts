import type { ManifestTiptapExtension } from './tiptap-extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export interface UmbTiptapExtensionApi extends UmbApi {
	getTiptapExtensions(args?: UmbTiptapExtensionArgs): Array<Extension | Mark | Node>;
}

export abstract class UmbTiptapExtensionApiBase extends UmbControllerBase implements UmbTiptapExtensionApi {
	public manifest?: ManifestTiptapExtension;

	abstract getTiptapExtensions(args?: UmbTiptapExtensionArgs): Array<Extension | Mark | Node>;
}

export interface UmbTiptapExtensionArgs {
	/**
	 * The data type configuration for the property editor that the editor is used for.
	 * You can populate this manually if you are using the editor outside of a property editor with the {@link UmbPropertyEditorConfigCollection} object.
	 * @remark This is only available when the editor is used in a property editor or populated manually.
	 */
	configuration?: UmbPropertyEditorConfigCollection;
}

export interface UmbTiptapToolbarElementApi extends UmbTiptapExtensionApi {
	execute(editor?: Editor): void;
	isActive(editor?: Editor): boolean;
}

export abstract class UmbTiptapToolbarElementApiBase
	extends UmbTiptapExtensionApiBase
	implements UmbTiptapToolbarElementApi
{
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public execute(editor?: Editor) {}

	public isActive(editor?: Editor) {
		return editor && this.manifest?.meta.alias ? editor?.isActive(this.manifest.meta.alias) : false;
	}
}
