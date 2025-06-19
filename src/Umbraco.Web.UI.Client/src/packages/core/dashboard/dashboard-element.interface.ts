import type { ManifestDashboard } from './dashboard.extension.js';

export interface UmbDashboardElement extends HTMLElement {
	manifest?: ManifestDashboard;
}
