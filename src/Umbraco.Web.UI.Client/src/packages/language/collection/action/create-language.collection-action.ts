import { UmbCollectionActionBase } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateLanguageCollectionAction extends UmbCollectionActionBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async execute() {
		alert('HELLO');
	}
}
