import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class TrashMediaEntityAction {
	#host: UmbControllerHostInterface;
	#key: string;

	constructor(host: UmbControllerHostInterface, key: string) {
		this.#host = host;
		this.#key = key;
	}

	async execute() {
		alert('trash media');
	}
}
