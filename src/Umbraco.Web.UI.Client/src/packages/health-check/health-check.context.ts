import { BehaviorSubject } from '@umbraco-cms/backoffice/external/rxjs';
import type {
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { HealthCheckResource } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbHealthCheckContext extends UmbControllerBase implements UmbApi {
	private _checks = new BehaviorSubject<HealthCheckGroupPresentationModel | undefined>(undefined);
	public readonly checks = this._checks.asObservable();

	private _results = new BehaviorSubject<HealthCheckGroupWithResultResponseModel | undefined>(undefined);
	public readonly results = this._results.asObservable();

	async getGroupChecks(name: string) {
		const { data } = await tryExecuteAndNotify(this, HealthCheckResource.getHealthCheckGroupByName({ name }));

		if (data) {
			this._checks.next(data);
		} else {
			this._checks.next(undefined);
		}
	}

	async checkGroup(name: string) {
		const { data } = await tryExecuteAndNotify(this, HealthCheckResource.postHealthCheckGroupByNameCheck({ name }));

		if (data) {
			this._results.next(data);
		} else {
			this._results.next(undefined);
		}
	}

	static isInstanceLike(instance: unknown): instance is UmbHealthCheckContext {
		return typeof instance === 'object' && (instance as UmbHealthCheckContext).results !== undefined;
	}
}

export default UmbHealthCheckContext;

export const UMB_HEALTHCHECK_CONTEXT = new UmbContextToken<UmbHealthCheckContext>('UmbHealthCheckContext');
