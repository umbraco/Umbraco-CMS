import type {
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootItemsRequestArgs,
	UmbDocumentTreeRootModel,
} from './types.js';
import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbDefaultTreeContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTreeContext extends UmbDefaultTreeContext<
	UmbDocumentTreeItemModel,
	UmbDocumentTreeRootModel,
	UmbDocumentTreeRootItemsRequestArgs
> {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context?.dataType,
				(value) => {
					if (value === undefined) return;
					this.updateAdditionalRequestArgs({ dataType: value });
				},
				'umbDocumentTreeDataTypeObserver',
			);
		});
	}
}

export { UmbDocumentTreeContext as api };
