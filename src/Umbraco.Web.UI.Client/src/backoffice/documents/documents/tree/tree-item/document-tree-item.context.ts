import { UmbTreeItemContextBase } from '../../../../shared/components/tree/tree-item-base/tree-item-base.context';
import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO get unique method from an document repository static method
export class UmbDocumentTreeItemContext extends UmbTreeItemContextBase<DocumentTreeItemResponseModel> {
	constructor(host: UmbControllerHostInterface, treeItem: DocumentTreeItemResponseModel) {
		super(host, treeItem, (x: DocumentTreeItemResponseModel) => x.key);
	}
}
