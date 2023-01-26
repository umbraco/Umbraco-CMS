import { BehaviorSubject, Observable } from 'rxjs';
import { HealthCheckResource, HealthCheckWithResult } from '@umbraco-cms/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextToken } from '@umbraco-cms/context-api';

export class UmbHealthCheckContext {
	private _checks: BehaviorSubject<Array<any>> = new BehaviorSubject(<Array<any>>[]);
	public readonly checks: Observable<Array<any>> = this._checks.asObservable();

	private _results: BehaviorSubject<Array<any>> = new BehaviorSubject(<Array<any>>[]);
	public readonly results: Observable<Array<any>> = this._results.asObservable();

	public host: UmbControllerHostInterface;

	constructor(host: UmbControllerHostInterface) {
		this.host = host;
	}

	//TODO: Is this how we want to it?

	async getGroupChecks(name: string) {
		const { data } = await tryExecuteAndNotify(this.host, HealthCheckResource.getHealthCheckGroupByName({ name }));

		if (data) {
			data.checks?.forEach((check) => {
				delete check.results;
			});
			this._checks.next(data.checks as HealthCheckWithResult[]);
		}
	}

	async checkGroup(name: string) {
		const { data } = await tryExecuteAndNotify(this.host, HealthCheckResource.getHealthCheckGroupByName({ name }));

		if (data) {
			const results =
				data.checks?.map((check) => {
					return {
						key: check.key,
						results: check.results,
					};
				}) || [];

			this._results.next(results);
		}
	}
}

export const UMB_HEALTHCHECK_CONTEXT_TOKEN = new UmbContextToken<UmbHealthCheckContext>(UmbHealthCheckContext.name);
