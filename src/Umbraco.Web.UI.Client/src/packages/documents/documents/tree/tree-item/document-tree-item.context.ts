import { UmbDocumentTreeItemModel } from '../types.js';
import { UmbUniqueTreeItemContext } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentTreeItemContext extends UmbUniqueTreeItemContext<UmbDocumentTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host);
	}
}
