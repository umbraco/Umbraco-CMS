import type { ExampleTreeItemModel, ExampleTreeRootModel } from '../../types.js';
import { EXAMPLE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import { ExampleTreeLocalDataSource } from '../local-data-source/index.js';
import { UmbTreeRepositoryBase, type UmbTreeRepository } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class ExampleTreeRepository
	extends UmbTreeRepositoryBase<ExampleTreeItemModel, ExampleTreeRootModel>
	implements UmbTreeRepository, UmbApi
{
	constructor(host: UmbControllerHost) {
		super(host, ExampleTreeLocalDataSource);
	}

	async requestTreeRoot() {
		const root: ExampleTreeRootModel = {
			unique: null,
			entityType: EXAMPLE_ROOT_ENTITY_TYPE,
			name: 'Example Tree',
			hasChildren: true,
			isFolder: true,
		};

		return { data: root };
	}
}

export { ExampleTreeRepository as api };
