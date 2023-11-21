import { type UmbCollectionContext, UMB_COLLECTION_CONTEXT } from '../index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionAction<CollectionContextType = unknown> extends UmbApi {
	host: UmbControllerHost;
	collectionContext?: CollectionContextType;
	execute(): Promise<void>;
}

export abstract class UmbCollectionActionBase<CollectionContextType extends UmbCollectionContext<any, any>>
	implements UmbCollectionAction<CollectionContextType>
{
	host: UmbControllerHost;
	collectionContext?: CollectionContextType;
	constructor(host: UmbControllerHost) {
		this.host = host;

		new UmbContextConsumerController(this.host, UMB_COLLECTION_CONTEXT, (instance) => {
			// TODO: Be aware we are casting here. We should consider a better solution for typing the contexts. (But notice we still want to capture the collection workspace...)
			this.collectionContext = instance as unknown as CollectionContextType;
		});
	}
	abstract execute(): Promise<void>;
}
