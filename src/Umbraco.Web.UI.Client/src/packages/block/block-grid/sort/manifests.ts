import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../property-editors/constants.js';
import { UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS];

const propertyContext: UmbExtensionManifest = {
	type: 'propertyContext',
	kind: 'sort',
	alias: 'Umb.PropertyContext.BlockGrid.Sort',
	name: 'Block Grid Sort Property Context',
	forPropertyEditorUis,
};

const propertyAction: UmbExtensionManifest = {
	type: 'propertyAction',
	kind: 'sortMode',
	alias: 'Umb.PropertyAction.BlockGrid.SortMode',
	name: 'Block Grid Sort Mode Property Action',
	forPropertyEditorUis,
	conditions: [
		{
			alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [propertyContext, propertyAction];
