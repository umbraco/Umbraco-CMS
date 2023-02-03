import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSortChildrenOfDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
	}

	execute() {
		alert('Sort');
	}
}
