import type { UmbDocumentTreeItemModel, UmbDocumentTreeRootModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CONTENT_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UmbDefaultTreeContext } from '@umbraco-cms/backoffice/tree';

export class UmbDocumentTreeContext extends UmbDefaultTreeContext<UmbDocumentTreeItemModel, UmbDocumentTreeRootModel> {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_CONTENT_PROPERTY_CONTEXT, (context) => {
			console.log(context);
		});
	}
}

export { UmbDocumentTreeContext as api };
