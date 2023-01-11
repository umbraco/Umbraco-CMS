import { HealthCheckResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';

export class UmbHealthCheckContext {
	host = null;
	constructor(host: any) {
		this.host = host;
	}

	hej() {
		console.log('hej');
	}

	async checkGroup(name: string) {
		const response = await HealthCheckResource.getHealthCheckGroupByName({ name });

		const results = response.checks?.map((check) => {
			return {
				key: check.key,
				results: check.results,
			};
		});

		if (response) return results;
	}

	async getGroupChecks(name: string) {
		const response = await HealthCheckResource.getHealthCheckGroupByName({ name });
		response.checks?.forEach((check) => {
			delete check.results;
		});
		if (response) return response.checks;
	}
}
