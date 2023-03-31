import { BehaviorSubject } from 'rxjs';
import {
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
	HealthCheckResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbHealthCheckContext {
	private _checks = new BehaviorSubject<HealthCheckGroupPresentationModel | undefined>(undefined);
	public readonly checks = this._checks.asObservable();

	private _results = new BehaviorSubject<HealthCheckGroupWithResultResponseModel | undefined>(undefined);
	public readonly results = this._results.asObservable();

	public host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
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

export const UMB_HEALTHCHECK_CONTEXT_TOKEN = new UmbContextToken<UmbHealthCheckContext>('UmbHealthCheckContext');
