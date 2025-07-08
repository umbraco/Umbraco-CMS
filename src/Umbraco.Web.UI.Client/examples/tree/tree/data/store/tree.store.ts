import { EXAMPLE_TREE_STORE_CONTEXT } from './tree.store.context-token.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class ExampleTreeStore extends UmbUniqueTreeStore {
	constructor(host: UmbControllerHost) {
		super(host, EXAMPLE_TREE_STORE_CONTEXT.toString());
	}
}

export { ExampleTreeStore as api };
