import type { UmbHealthCheckContext } from './health-check.context.js';
import type { ManifestHealthCheck } from '@umbraco-cms/backoffice/extension-registry';
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

	checkAll() {
		for (const [label, api] of this.apis.entries()) {
			api?.checkGroup?.(label);
		}
	}

	#registerApis() {
		this.apis.clear();
		this.#manifests.forEach(async (manifest) => {
			const api = await loadManifestApi<UmbHealthCheckContext>(manifest.meta.api);
			if (api) this.apis.set(manifest.meta.label, new api(this.host));
		});
	}
}

export const UMB_HEALTHCHECK_DASHBOARD_CONTEXT = new UmbContextToken<UmbHealthCheckDashboardContext>(
	'UmbHealthCheckDashboardContext',
);
