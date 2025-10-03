import type { ManifestDashboard } from './dashboard.extension.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export interface UmbDashboardElement extends UmbControllerHostElement {
	manifest?: ManifestDashboard;
}
