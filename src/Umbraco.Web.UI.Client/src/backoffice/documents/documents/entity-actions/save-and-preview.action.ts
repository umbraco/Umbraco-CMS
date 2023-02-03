import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbSaveAndPreviewDocumentEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
	}

	execute() {
		alert('save and preview');
	}
}
