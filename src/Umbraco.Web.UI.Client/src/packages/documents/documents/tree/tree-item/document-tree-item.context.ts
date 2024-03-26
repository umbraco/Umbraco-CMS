import type { UmbDocumentTreeItemModel } from '../types.js';
import { UmbDefaultTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTreeItemContext extends UmbDefaultTreeItemContext<UmbDocumentTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
