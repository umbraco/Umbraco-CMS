import type { ManifestTiptapExtension } from './tiptap-extension.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export interface UmbTiptapExtensionApi extends UmbApi {
	/**
	 * Sets the editor instance to the extension.
	 */
	setEditor(editor: Editor): void;

	/**
	 * Gets the Tiptap extensions for the editor.
	 */
	getTiptapExtensions(args?: UmbTiptapExtensionArgs): Array<Extension | Mark | Node>;
}

export abstract class UmbTiptapExtensionApiBase extends UmbControllerBase implements UmbTiptapExtensionApi {
	/**
	 * The manifest for the extension.
	 */
	protected _manifest?: ManifestTiptapExtension;

	/**
	 * The editor instance.
	 */
	protected _editor?: Editor;

	/**
	 * @inheritdoc
	 */
	setEditor(editor: Editor): void {
		this._editor = editor;
	}

	/**
	 * @inheritdoc
	 */
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
	/**
	 * Executes the toolbar element action.
	 */
	execute(editor: Editor): void;

	/**
	 * Checks if the toolbar element is active.
	 */
	isActive(editor: Editor): boolean;
}

export abstract class UmbTiptapToolbarElementApiBase
	extends UmbTiptapExtensionApiBase
	implements UmbTiptapToolbarElementApi
{
	/**
	 * A method to execute the toolbar element action.
	 */
	public abstract execute(editor: Editor): void;

	/**
	 * Informs the toolbar element if it is active or not. It uses the manifest meta alias to check if the toolbar element is active.
	 * @see {ManifestTiptapExtension}
	 */
	public isActive(editor?: Editor) {
		return editor && this._manifest?.meta.alias ? editor?.isActive(this._manifest.meta.alias) : false;
	}
}
