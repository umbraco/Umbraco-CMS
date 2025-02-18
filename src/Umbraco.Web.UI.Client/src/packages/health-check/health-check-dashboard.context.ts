import { UmbHealthCheckContext } from './health-check.context.js';
import type { ManifestHealthCheck } from './health-check.extension.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbHealthCheckDashboardContext {
	#manifests: ManifestHealthCheck[] = [];
	set manifests(value: ManifestHealthCheck[]) {
		this.#manifests = value;
		this.#registerApis();
	}
	get manifests() {
		return this.#manifests;
	}

	public apis = new Map<string, UmbHealthCheckContext>();
	public host: HTMLElement;

	constructor(host: HTMLElement) {
		this.host = host;
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
			const apiInstance = new api(this.host);
			if (api && UmbHealthCheckContext.isInstanceLike(apiInstance)) this.apis.set(manifest.meta.label, apiInstance);
		});
	}
}

export const UMB_HEALTHCHECK_DASHBOARD_CONTEXT = new UmbContextToken<UmbHealthCheckDashboardContext>(
	'UmbHealthCheckDashboardContext',
);
