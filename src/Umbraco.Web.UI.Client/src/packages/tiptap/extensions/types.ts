import type { ManifestTiptapExtension } from './tiptap.extension.js';
import type { ManifestTiptapToolbarExtension } from './tiptap-toolbar.extension.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

export type * from './tiptap.extension.js';
export type * from './tiptap-toolbar.extension.js';

export interface UmbTiptapExtensionApi extends UmbApi {
	/**
	 * The manifest for the extension.
	 */
	manifest?: ManifestTiptapExtension;

	/**
	 * Sets the editor instance to the extension.
	 */
	setEditor(editor: Editor): void;

	/**
	 * Gets the styles for the extension
	 */
	getStyles(): CSSResultGroup | null | undefined;

	/**
	 * Gets the Tiptap extensions for the editor.
	 */
	getTiptapExtensions(args?: UmbTiptapExtensionArgs): Array<Extension | Mark | Node>;
}

export interface UmbTiptapExtensionArgs {
	/**
	 * The data type configuration for the property editor that the editor is used for.
	 * You can populate this manually if you are using the editor outside of a property editor with the {@link UmbPropertyEditorConfigCollection} object.
	 * @remark This is only available when the editor is used in a property editor or populated manually.
	 */
	configuration?: UmbPropertyEditorConfigCollection;
}

export interface UmbTiptapToolbarElementApi extends UmbApi, UmbTiptapExtensionArgs {
	/**
	 * The manifest for the extension.
	 */
	manifest?: ManifestTiptapToolbarExtension;

	/**
	 * Executes the toolbar element action.
	 */
	execute(editor?: Editor, ...args: Array<unknown>): void;

	/**
	 * Checks if the toolbar element is active.
	 */
	isActive(editor?: Editor): boolean;

	/**
	 * Checks if the toolbar element is disabled.
	 */
	isDisabled(editor?: Editor): boolean;
}
