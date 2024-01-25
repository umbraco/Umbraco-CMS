import type { UmbDocumentTreeItemModel } from '../types.js';
import { UmbTreeItemContextBase } from '@umbraco-cms/backoffice/tree';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// TODO get unique method from an document repository static method
export class UmbDocumentTreeItemContext extends UmbTreeItemContextBase<UmbDocumentTreeItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, (x: UmbDocumentTreeItemModel) => x.id);
	}
}
