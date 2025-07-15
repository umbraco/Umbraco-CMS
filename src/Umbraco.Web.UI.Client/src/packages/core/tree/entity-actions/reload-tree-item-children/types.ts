import type { ManifestEntityAction, MetaEntityActionDefaultKind } from '@umbraco-cms/backoffice/entity-action';

export interface ManifestEntityActionReloadTreeItemChildrenKind
	extends ManifestEntityAction<MetaEntityActionReloadTreeItemChildrenKind> {
	type: 'entityAction';
	kind: 'reloadTreeItemChildren';
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityActionReloadTreeItemChildrenKind extends MetaEntityActionDefaultKind {}

declare global {
	interface UmbExtensionManifestMap {
		umbReloadTreeItemChildrenEntityAction: ManifestEntityActionReloadTreeItemChildrenKind;
	}
}
