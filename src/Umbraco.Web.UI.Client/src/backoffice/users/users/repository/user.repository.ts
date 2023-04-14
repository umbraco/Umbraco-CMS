import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

// TODO: implement
export class UmbUserRepository {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		console.log(this.#host);
	}
}
