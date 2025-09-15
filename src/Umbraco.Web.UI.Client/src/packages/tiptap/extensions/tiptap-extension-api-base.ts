import type { ManifestTiptapExtension, UmbTiptapExtensionApi, UmbTiptapExtensionArgs } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import type { Editor, Extension, Mark, Node } from '@umbraco-cms/backoffice/external/tiptap';

export abstract class UmbTiptapExtensionApiBase extends UmbControllerBase implements UmbTiptapExtensionApi {
	/**
	 * The manifest for the extension.
	 */
	manifest?: ManifestTiptapExtension;

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
	getStyles(): CSSResultGroup | null | undefined {
		return null;
	}

	/**
	 * @inheritdoc
	 */
	abstract getTiptapExtensions(args?: UmbTiptapExtensionArgs): Array<Extension | Mark | Node>;
}
