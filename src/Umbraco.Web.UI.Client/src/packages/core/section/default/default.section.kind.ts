import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbDefaultSectionContext from './default-section.context.js';
import { UmbDefaultSectionElement } from './default-section.element.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Section.Default',
	matchKind: 'default',
	matchType: 'section',
	manifest: {
		type: 'section',
		kind: 'default',
		weight: 1000,
		api: UmbDefaultSectionContext,
		element: UmbDefaultSectionElement,
		meta: {
			label: '',
			pathname: '',
		},
	},
};
