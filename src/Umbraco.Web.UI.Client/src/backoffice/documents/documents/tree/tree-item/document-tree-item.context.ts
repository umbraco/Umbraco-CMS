import { UmbTreeItemContextBase } from '../../../../shared/components/tree/tree-item-base/tree-item-base.context';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO get unique method from an document repository static method
export class UmbDocumentTreeItemContext extends UmbTreeItemContextBase<DocumentTreeItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, (x: DocumentTreeItemResponseModel) => x.key);
	}
}
