import { UmbHealthCheckContext } from './health-check.context.js';
import type { ManifestHealthCheck } from './health-check.extension.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbHealthCheckDashboardContext extends UmbContextBase {
	#manifests: ManifestHealthCheck[] = [];
	set manifests(value: ManifestHealthCheck[]) {
		this.#manifests = value;
		this.#registerApis();
	}
	get manifests() {
		return this.#manifests;
	}

	public apis = new Map<string, UmbHealthCheckContext>();

	constructor(host: UmbControllerHost) {
		super(host, UMB_HEALTHCHECK_DASHBOARD_CONTEXT);
	}

	async checkAll() {
		for (const [label, api] of this.apis.entries()) {
			await api?.checkGroup?.(label);
		}
	}

	#registerApis() {
		this.apis.clear();
		this.#manifests.forEach(async (manifest) => {
			if (!manifest.api) return;
			const api = await loadManifestApi(manifest.api);
			if (!api) return;
			const apiInstance = new api(this);
			if (api && UmbHealthCheckContext.isInstanceLike(apiInstance)) this.apis.set(manifest.meta.label, apiInstance);
		});
	}
}

export const UMB_HEALTHCHECK_DASHBOARD_CONTEXT = new UmbContextToken<UmbHealthCheckDashboardContext>(
	'UmbHealthCheckDashboardContext',
);
