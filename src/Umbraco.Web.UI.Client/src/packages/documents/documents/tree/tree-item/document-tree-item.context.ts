import { UmbTreeItemContextBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO get unique method from an document repository static method
export class UmbDocumentTreeItemContext extends UmbTreeItemContextBase<DocumentTreeItemResponseModel> {
	constructor(host: UmbControllerHost) {
		super(host, (x: DocumentTreeItemResponseModel) => x.id);
	}
}
