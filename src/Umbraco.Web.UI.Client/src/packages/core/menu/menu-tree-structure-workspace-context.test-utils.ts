import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

@customElement('umb-test-menu-structure-controller-host')
export class UmbTestMenuStructureControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * A minimal `UmbSubmittableTreeEntityWorkspaceContext` stand-in — stands in for a real document/document-type/etc.
 * workspace context in isolated tests. Only implements what `UmbMenuTreeStructureWorkspaceContextBase` reads, plus
 * the two properties (`requestSubmit`, `_internal_createUnderParent`) the context token's discriminator checks for.
 */
export class UmbTestSubmittableTreeEntityWorkspaceContext {
	#host: UmbControllerHost;
	#unique = new UmbObjectState<string | null | undefined>(undefined);
	#entityType = new UmbStringState<string | undefined>(undefined);
	#isNew = new UmbBooleanState(undefined);
	#createUnderParentUnique = new UmbObjectState<string | null | undefined>(undefined);
	#createUnderParentEntityType = new UmbStringState<string | undefined>(undefined);

	readonly workspaceAlias = 'Umb.Test.Workspace';
	readonly unique = this.#unique.asObservable();
	readonly entityType = this.#entityType.asObservable();
	readonly isNew = this.#isNew.asObservable();
	// eslint-disable-next-line @typescript-eslint/naming-convention
	readonly _internal_createUnderParent = new UmbObjectState<UmbEntityModel | undefined>(undefined).asObservable();
	// eslint-disable-next-line @typescript-eslint/naming-convention
	readonly _internal_createUnderParentEntityUnique = this.#createUnderParentUnique.asObservable();
	// eslint-disable-next-line @typescript-eslint/naming-convention
	readonly _internal_createUnderParentEntityType = this.#createUnderParentEntityType.asObservable();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	getUnique() {
		return this.#unique.getValue();
	}

	getEntityType() {
		return this.#entityType.getValue();
	}

	getIsNew() {
		return this.#isNew.getValue();
	}

	setUnique(unique: string | null) {
		this.#unique.setValue(unique);
	}

	setEntityType(entityType: string) {
		this.#entityType.setValue(entityType);
	}

	setIsNew(value: boolean | undefined) {
		this.#isNew.setValue(value);
	}

	setCreateUnderParent(parent: UmbEntityModel) {
		this.#createUnderParentUnique.setValue(parent.unique);
		this.#createUnderParentEntityType.setValue(parent.entityType);
	}

	async requestSubmit() {}

	destroy() {}
}

/**
 * A minimal `UmbTreeRepository` stand-in, registered into `umbExtensionsRegistry` under whichever alias the
 * context-under-test expects. Root and ancestors are static so a test can configure the response before
 * triggering a structure request, regardless of which repository alias/instance ends up used.
 */
const DEFAULT_TEST_ROOT: UmbTreeRootModel = {
	unique: null,
	entityType: 'test-root-entity-type',
	name: 'Root',
	isFolder: false,
	hasChildren: false,
} as unknown as UmbTreeRootModel;

export class UmbTestTreeRepository {
	static root: UmbTreeRootModel = DEFAULT_TEST_ROOT;
	static ancestors: Array<UmbTreeItemModel> = [];
	static requestTreeItemAncestorsCalls: Array<UmbEntityModel> = [];

	static reset() {
		UmbTestTreeRepository.root = DEFAULT_TEST_ROOT;
		UmbTestTreeRepository.ancestors = [];
		UmbTestTreeRepository.requestTreeItemAncestorsCalls = [];
	}

	async requestTreeRoot() {
		return { data: UmbTestTreeRepository.root };
	}

	async requestTreeItemAncestors(args: { treeItem: UmbEntityModel }) {
		UmbTestTreeRepository.requestTreeItemAncestorsCalls.push(args.treeItem);
		return { data: UmbTestTreeRepository.ancestors };
	}

	destroy() {}
}

export const createTestTreeRepositoryManifest = (alias: string): ManifestApi => ({
	type: 'repository',
	alias,
	name: 'Test Tree Repository',
	api: UmbTestTreeRepository,
});

export function createTestAncestorItem(entity: UmbEntityModel, name = entity.unique ?? 'unnamed'): UmbTreeItemModel {
	return {
		...entity,
		name,
		isFolder: false,
		hasChildren: false,
		parent: { unique: null, entityType: 'test-root-entity-type' },
	} as unknown as UmbTreeItemModel;
}
