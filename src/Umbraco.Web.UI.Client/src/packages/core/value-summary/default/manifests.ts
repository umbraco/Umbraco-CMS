import { UmbDefaultValueSummaryElement } from './default-value-summary.element.js';
import { UmbValueSummaryDefaultApi } from './default-value-summary.api.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.ValueSummary.Default',
		matchKind: 'default',
		matchType: 'valueSummary',
		manifest: {
			type: 'valueSummary',
			kind: 'default',
			element: UmbDefaultValueSummaryElement,
			api: UmbValueSummaryDefaultApi,
		},
	},
];
