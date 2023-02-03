import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbMoveDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
	}

	execute() {
		alert('move');
	}
}
