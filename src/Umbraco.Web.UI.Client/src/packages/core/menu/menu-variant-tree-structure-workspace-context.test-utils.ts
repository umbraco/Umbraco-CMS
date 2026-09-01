import { UmbTestSubmittableTreeEntityWorkspaceContext } from './menu-tree-structure-workspace-context.test-utils.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export { UmbTestSubmittableTreeEntityWorkspaceContext };

@customElement('umb-test-menu-variant-structure-controller-host')
export class UmbTestMenuVariantStructureControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

type UmbTestVariantTreeItemModel = UmbTreeItemModel & {
	variants: Array<{ name: string; culture: string | null; segment: string | null }>;
};

/**
 * A minimal `UmbTreeRepository` stand-in for a variant-aware tree (document/media). Same role as
 * `UmbTestTreeRepository` in `menu-tree-structure-workspace-context.test-utils.ts`, but its ancestors carry a
 * `variants` array, since `UmbMenuVariantTreeStructureWorkspaceContextBase` reads `treeItem.variants` when
 * building the structure.
 */
const DEFAULT_TEST_ROOT: UmbTreeRootModel = {
	unique: null,
	entityType: 'test-root-entity-type',
	name: 'Root',
	isFolder: false,
	hasChildren: false,
} as unknown as UmbTreeRootModel;

export class UmbTestVariantTreeRepository {
	static root: UmbTreeRootModel = DEFAULT_TEST_ROOT;
	static ancestors: Array<UmbTestVariantTreeItemModel> = [];
	static requestTreeItemAncestorsCalls: Array<UmbEntityModel> = [];

	static reset() {
		UmbTestVariantTreeRepository.root = DEFAULT_TEST_ROOT;
		UmbTestVariantTreeRepository.ancestors = [];
		UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls = [];
	}

	async requestTreeRoot() {
		return { data: UmbTestVariantTreeRepository.root };
	}

	async requestTreeItemAncestors(args: { treeItem: UmbEntityModel }) {
		UmbTestVariantTreeRepository.requestTreeItemAncestorsCalls.push(args.treeItem);
		return { data: UmbTestVariantTreeRepository.ancestors };
	}

	destroy() {}
}

export const createTestVariantTreeRepositoryManifest = (alias: string): ManifestApi => ({
	type: 'repository',
	alias,
	name: 'Test Variant Tree Repository',
	api: UmbTestVariantTreeRepository,
});

export function createTestVariantAncestorItem(
	entity: UmbEntityModel,
	name = entity.unique ?? 'unnamed',
): UmbTestVariantTreeItemModel {
	return {
		...entity,
		name,
		isFolder: false,
		hasChildren: false,
		parent: { unique: null, entityType: 'test-root-entity-type' },
		variants: [{ name, culture: null, segment: null }],
	} as unknown as UmbTestVariantTreeItemModel;
}
