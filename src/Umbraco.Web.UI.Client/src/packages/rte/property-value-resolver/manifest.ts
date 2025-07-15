import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { UmbRteBlockValueResolver } from './rte-block-value-resolver.api.js';
import type { ManifestPropertyValueResolver } from '@umbraco-cms/backoffice/property';

export const manifest: ManifestPropertyValueResolver = {
	type: 'propertyValueResolver',
	alias: 'Umb.PropertyValueResolver.RichTextBlocks',
	name: 'Block Value Resolver',
	api: UmbRteBlockValueResolver,
	forEditorAlias: UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
};
