import { UmbTreeItemContextBase } from '@umbraco-cms/backoffice/tree';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO get unique method from an document repository static method
export class UmbDocumentTreeItemContext extends UmbTreeItemContextBase<DocumentTreeItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: DocumentTreeItemResponseModel) => x.id);
	}
}
