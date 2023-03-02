import { UmbControllerHostInterface } from '@umbraco-cms/controller';

// TODO: implement
export class UmbUserGroupRepository {
	#host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		console.log(this.#host);
	}
}
