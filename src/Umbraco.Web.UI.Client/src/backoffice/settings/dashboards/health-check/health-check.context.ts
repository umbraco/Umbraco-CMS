import { BehaviorSubject } from 'rxjs';
import { HealthCheckGroupModel, HealthCheckGroupWithResultModel, HealthCheckResource } from '@umbraco-cms/backend-api';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

export class UmbHealthCheckContext {
	private _checks = new BehaviorSubject<HealthCheckGroupModel | undefined>(undefined);
	public readonly checks = this._checks.asObservable();

	private _results = new BehaviorSubject<HealthCheckGroupWithResultModel | undefined>(undefined);
	public readonly results = this._results.asObservable();

	public host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.host = host;
	}

	async getGroupChecks(name: string) {
		const { data } = await tryExecuteAndNotify(this.host, HealthCheckResource.getHealthCheckGroupByName({ name }));

		if (data) {
			this._checks.next(data);
		} else {
			this._checks.next(undefined);
		}
	}

	async checkGroup(name: string) {
		const { data } = await tryExecuteAndNotify(
			this.host,
			HealthCheckResource.postHealthCheckGroupByNameCheck({ name })
		);

		if (data) {
			this._results.next(data);
		} else {
			this._results.next(undefined);
		}
	}
}

export const UMB_HEALTHCHECK_CONTEXT_TOKEN = new UmbContextToken<UmbHealthCheckContext>(UmbHealthCheckContext.name);
