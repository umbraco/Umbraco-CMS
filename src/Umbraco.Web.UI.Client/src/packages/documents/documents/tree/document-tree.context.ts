import type {
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
	UmbDocumentTreeRootModel,
} from './types.js';
import { UMB_CONTENT_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbDefaultTreeContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTreeContext extends UmbDefaultTreeContext<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel,
	UmbDocumentTreeRootItemsRequestArgs
> {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CONTENT_PROPERTY_CONTEXT, (context) => {
			this.observe(context.dataType, (value) => {
				this.updateAdditionalRequestArgs({ dataType: value });
			});
		});
	}
}

export { UmbDocumentTreeContext as api };
