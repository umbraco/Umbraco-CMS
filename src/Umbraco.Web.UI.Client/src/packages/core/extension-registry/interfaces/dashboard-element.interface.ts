import type { ManifestDashboard } from '../models/index.js';

export interface UmbDashboardElement extends HTMLElement {
	manifest?: ManifestDashboard;
}
