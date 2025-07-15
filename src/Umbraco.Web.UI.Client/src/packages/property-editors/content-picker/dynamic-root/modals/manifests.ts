import {
	UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
	UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CONTENT_PICKER_DOCUMENT_ROOT_ORIGIN_PICKER_MODAL_ALIAS,
		name: 'Choose an origin',
		element: () => import('./dynamic-root-origin-picker-modal.element.js'),
	},
	{
		type: 'modal',
		alias: UMB_CONTENT_PICKER_DOCUMENT_ROOT_QUERY_STEP_PICKER_MODAL_ALIAS,
		name: 'Append step to query',
		element: () => import('./dynamic-root-query-step-picker-modal.element.js'),
	},
	{
		type: 'dynamicRootOrigin',
		alias: 'Umb.DynamicRootOrigin.Root',
		name: 'Dynamic Root Origin: Root',
		meta: {
			originAlias: 'Root',
			label: 'Root',
			description: 'Root node of this editing session.',
			icon: 'icon-home',
		},
		weight: 100,
	},
	{
		type: 'dynamicRootOrigin',
		alias: 'Umb.DynamicRootOrigin.Parent',
		name: 'Dynamic Root Origin: Parent',
		meta: {
			originAlias: 'Parent',
			label: 'Parent',
			description: 'The parent node of the source in this editing session.',
			icon: 'icon-page-up',
		},
		weight: 90,
	},
	{
		type: 'dynamicRootOrigin',
		alias: 'Umb.DynamicRootOrigin.Current',
		name: 'Dynamic Root Origin: Current',
		meta: {
			originAlias: 'Current',
			label: 'Current',
			description: 'The content node that is source for this editing session.',
			icon: 'icon-document',
		},
		weight: 80,
	},
	{
		type: 'dynamicRootOrigin',
		alias: 'Umb.DynamicRootOrigin.Site',
		name: 'Dynamic Root Origin: Site',
		meta: {
			originAlias: 'Site',
			label: 'Site',
			description: 'Find nearest node with a hostname.',
			icon: 'icon-home',
		},
		weight: 70,
	},
	{
		type: 'dynamicRootOrigin',
		alias: 'Umb.DynamicRootOrigin.ByKey',
		name: 'Dynamic Root Origin: By Key',
		meta: {
			originAlias: 'ByKey',
			label: 'Specific Node',
			description: 'Pick a specific Node as the origin for this query.',
			icon: 'icon-wand',
		},
		weight: 60,
	},
	{
		type: 'dynamicRootQueryStep',
		alias: 'Umb.DynamicRootQueryStep.NearestAncestorOrSelf',
		name: 'Dynamic Root Query Step: Nearest Ancestor Or Self',
		meta: {
			queryStepAlias: 'NearestAncestorOrSelf',
			label: 'Nearest Ancestor Or Self',
			description: 'Query the nearest ancestor or self that fits with one of the configured types.',
			icon: 'icon-arrow-up',
		},
		weight: 100,
	},
	{
		type: 'dynamicRootQueryStep',
		alias: 'Umb.DynamicRootQueryStep.FurthestAncestorOrSelf',
		name: 'Dynamic Root Query Step: Furthest Ancestor Or Self',
		meta: {
			queryStepAlias: 'FurthestAncestorOrSelf',
			label: 'Furthest Ancestor Or Self',
			description: 'Query the furthest ancestor or self that fits with one of the configured types.',
			icon: 'icon-arrow-up',
		},
		weight: 90,
	},
	{
		type: 'dynamicRootQueryStep',
		alias: 'Umb.DynamicRootQueryStep.NearestDescendantOrSelf',
		name: 'Dynamic Root Query Step: Nearest Descendant Or Self',
		meta: {
			queryStepAlias: 'NearestDescendantOrSelf',
			label: 'Nearest Descendant Or Self',
			description: 'Query the nearest descendant or self that fits with one of the configured types.',
			icon: 'icon-arrow-down',
		},
		weight: 80,
	},
	{
		type: 'dynamicRootQueryStep',
		alias: 'Umb.DynamicRootQueryStep.FurthestDescendantOrSelf',
		name: 'Dynamic Root Query Step: Furthest Descendant Or Self',
		meta: {
			queryStepAlias: 'FurthestDescendantOrSelf',
			label: 'Furthest Descendant Or Self',
			description: 'Query the furthest descendant or self that fits with one of the configured types.',
			icon: 'icon-arrow-down',
		},
		weight: 70,
	},
];
