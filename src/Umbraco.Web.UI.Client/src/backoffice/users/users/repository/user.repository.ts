import { UmbControllerHostInterface } from '@umbraco-cms/backoffice/controller';

// TODO: implement
export class UmbUserRepository {
	#host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		console.log(this.#host);
	}
}
