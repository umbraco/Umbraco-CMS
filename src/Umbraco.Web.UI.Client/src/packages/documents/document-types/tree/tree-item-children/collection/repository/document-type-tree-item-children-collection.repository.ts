import { UmbDocumentTypeTreeRepository } from '../../../document-type-tree.repository.js';
import type { UmbCollectionFilterModel, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export class UmbDocumentTypeTreeItemChildrenCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository
{
	#treeRepository = new UmbDocumentTypeTreeRepository(this);

	async requestCollection(filter: UmbCollectionFilterModel) {
		// TODO: get parent from args
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

export { UmbDocumentTypeTreeItemChildrenCollectionRepository as api };
