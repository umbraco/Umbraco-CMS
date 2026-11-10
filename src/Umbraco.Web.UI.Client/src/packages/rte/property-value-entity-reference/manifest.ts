import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { UmbRtePropertyValueEntityReferenceResolver } from './rte-property-value-entity-reference.resolver.js';
import type { ManifestPropertyValueEntityReference } from '@umbraco-cms/backoffice/property';

export const manifest: ManifestPropertyValueEntityReference = {
	type: 'propertyValueEntityReference',
	alias: 'Umb.PropertyValueEntityReference.RichText',
	name: 'Rich Text Entity Reference Resolver',
	api: UmbRtePropertyValueEntityReferenceResolver,
	forEditorAlias: UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
};
