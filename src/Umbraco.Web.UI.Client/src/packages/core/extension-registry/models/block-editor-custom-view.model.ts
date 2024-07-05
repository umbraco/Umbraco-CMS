import type { UmbBlockEditorCustomViewElement } from '../interfaces/index.js';
import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestBlockEditorCustomView extends ManifestElement<UmbBlockEditorCustomViewElement> {
	type: 'blockEditorCustomView';
	/**
	 * @property {string | Array<string> } - Declare if this Custom View only must appear at specific Content Types by Alias.
	 * @description Optional condition if you like this custom view to only appear at for one or more specific Content Types.
	 * @example 'my-element-type-alias'
	 * @example ['my-element-type-alias-A', 'my-element-type-alias-B']
	 */
	forContentTypeAlias?: string | Array<string>;
	/**
	 * @property {string | Array<string> } - Declare if this Custom View only must appear at specific Block Editors.
	 * @description Optional condition if you like this custom view to only appear at a specific type of Block Editor.
	 * @example 'block-list'
	 * @example ['block-list', 'block-grid']
	 */
	forBlockEditor?: string | Array<string>;
}
