import type { ManifestDashboard } from '../dashboard.extension.js';
import { UMB_DASHBOARD_CONTEXT } from './dashboard.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbDashboardContext extends UmbContextBase implements UmbApi {
	#manifestAlias = new UmbStringState<string | undefined>(undefined);
	#manifestPathname = new UmbStringState<string | undefined>(undefined);
	#manifestLabel = new UmbStringState<string | undefined>(undefined);
	public readonly alias = this.#manifestAlias.asObservable();
	public readonly pathname = this.#manifestPathname.asObservable();
	public readonly label = this.#manifestLabel.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DASHBOARD_CONTEXT);
	}

	public setManifest(manifest?: ManifestDashboard) {
		this.#manifestAlias.setValue(manifest?.alias);
		this.#manifestPathname.setValue(manifest?.meta?.pathname);
		this.#manifestLabel.setValue(manifest ? manifest.meta?.label || manifest.name : undefined);
	}
}

export { UmbDashboardContext as api };
