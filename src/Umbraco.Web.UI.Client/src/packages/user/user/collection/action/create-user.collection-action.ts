import { UmbUserCollectionContext } from '../user-collection.context.js';
import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateUserCollectionAction extends UmbCollectionActionBase<UmbUserCollectionContext> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	execute(): Promise<void> {
		throw new Error('Method not implemented.');
	}

	destroy(): void {
		throw new Error('Method not implemented.');
	}
}
