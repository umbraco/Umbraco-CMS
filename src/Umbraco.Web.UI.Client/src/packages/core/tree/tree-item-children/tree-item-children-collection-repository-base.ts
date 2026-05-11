import type { UmbTreeRepository } from '../data/index.js';
import type { UmbCollectionFilterModel, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

/**
 * Base class for a collection repository that provides the children of a tree item.
 * Resolves the parent entity from the entity context and delegates to the tree repository.
 * Call {@link setTreeRepositoryAlias} in the constructor of the subclass to configure which tree repository to use.
 * @abstract
 * @class UmbTreeItemChildrenCollectionRepositoryBase
 * @augments {UmbRepositoryBase}
 * @implements {UmbCollectionRepository}
 */
export abstract class UmbTreeItemChildrenCollectionRepositoryBase
	extends UmbRepositoryBase
	implements UmbCollectionRepository
{
	#treeRepository?: UmbTreeRepository;
	#init: Promise<unknown>;
	#initResolve!: (value: unknown) => void;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#init = new Promise((resolve) => {
			this.#initResolve = resolve;
		});
	}

	protected _setTreeRepositoryAlias(alias: string) {
		new UmbExtensionApiInitializer<ManifestRepository<UmbTreeRepository>>(
			this,
			umbExtensionsRegistry,
			alias,
			[this],
			(permitted, ctrl) => {
				this.#treeRepository = permitted ? ctrl.api : undefined;
				this.#initResolve(undefined);
			},
		);
	}

	async requestCollection(filter: UmbCollectionFilterModel = {}) {
		await this.#init;
		if (!this.#treeRepository) throw new Error('Tree repository is not available');

		const entityContext = await this.getContext(UMB_ENTITY_CONTEXT);
		if (!entityContext) throw new Error('Entity context not found');

		const entityType = entityContext.getEntityType();
		const unique = entityContext.getUnique();

		if (!entityType) throw new Error('Entity type not found');
		if (unique === undefined) throw new Error('Unique not found');

		const parent: UmbEntityModel = { entityType, unique };

		if (parent.unique === null) {
			return this.#treeRepository.requestTreeRootItems({ skip: filter.skip, take: filter.take });
		} else {
			return this.#treeRepository.requestTreeItemsOf({ parent, skip: filter.skip, take: filter.take });
		}
	}
}
