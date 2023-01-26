import { UmbHealthCheckContext } from './health-check.context';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';

export class UmbHealthCheckDashboardContext {
	public manifests: ManifestHealthCheck[] = [];
	public apis = new Map<string, UmbHealthCheckContext>();
	public host: any;

	constructor(host: any, manifests: Array<ManifestHealthCheck>) {
		this.manifests = manifests;
		this.host = host;

		this.manifests.forEach((manifest) => {
			this.apis.set(manifest.meta.label, new manifest.meta.api(this.host));
		});
	}

	checkAll() {
		for (const [label, api] of this.apis.entries()) {
			api.checkGroup(label);
		}
	}
}

export const UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN = new UmbContextToken<UmbHealthCheckDashboardContext>(
	UmbHealthCheckDashboardContext.name
);
