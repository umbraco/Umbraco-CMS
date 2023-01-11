import { BehaviorSubject, Observable } from 'rxjs';
import { HealthCheckResource, HealthCheckWithResult } from '@umbraco-cms/backend-api';

export class UmbHealthCheckContext {
	private _checks: BehaviorSubject<Array<any>> = new BehaviorSubject(<Array<any>>[]);
	public readonly checks: Observable<Array<any>> = this._checks.asObservable();

	private _results: BehaviorSubject<Array<any>> = new BehaviorSubject(<Array<any>>[]);
	public readonly results: Observable<Array<any>> = this._results.asObservable();

	public host = null;

	constructor(host: any) {
		this.host = host;
	}

	async getGroupChecks(name: string) {
		const response = await HealthCheckResource.getHealthCheckGroupByName({ name });
		response.checks?.forEach((check) => {
			delete check.results;
		});

		this._checks.next(response.checks as HealthCheckWithResult[]);
	}

	async checkGroup(name: string) {
		const response = await HealthCheckResource.getHealthCheckGroupByName({ name });

		const results =
			response.checks?.map((check) => {
				return {
					key: check.key,
					results: check.results,
				};
			}) || [];

		this._results.next(results);
	}
}
