import type { UmbBlockActionElement } from './block-action-element.interface.js';
import type { UmbBlockAction } from './block-action.interface.js';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * Manifest for a block action extension. Block actions appear in the action bar of block entries
 * (Block List, Block Grid, Block RTE, Block Single) and can be registered by both internal and
 * third-party extensions. Use `forBlockEditor` and `forContentTypeAlias` to filter which blocks
 * the action appears on.
 */
export interface ManifestBlockAction<MetaType extends MetaBlockAction = MetaBlockAction>
	extends ManifestElementAndApi<UmbBlockActionElement, UmbBlockAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'blockAction';
	/**
	 * @description Optional filter to only show this action for specific Content Types by alias.
	 * @example 'my-element-type-alias'
	 * @example ['my-element-type-alias-A', 'my-element-type-alias-B']
	 */
	forContentTypeAlias?: string | Array<string>;
	/**
	 * @description Optional filter to only show this action for specific Block Editors.
	 * @example 'block-list'
	 * @example ['block-list', 'block-grid']
	 */
	forBlockEditor?: string | Array<string>;
	meta: MetaType;
}

/**
 * Base metadata interface for block actions. Extend this to add custom metadata properties
 * for specific block action kinds.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaBlockAction {}

declare global {
	interface UmbExtensionManifestMap {
		umbBlockAction: ManifestBlockAction;
	}
}
