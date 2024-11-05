import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.CurrentUserAction.Default',
	matchKind: 'default',
	matchType: 'currentUserAction',
	manifest: {
		type: 'currentUserAction',
		kind: 'default',
		elementName: 'umb-current-user-app-button',
	},
};
