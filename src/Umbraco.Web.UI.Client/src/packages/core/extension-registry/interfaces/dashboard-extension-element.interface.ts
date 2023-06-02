import type { ManifestDashboard } from '../models/index.js';

export interface UmbDashboardExtensionElement extends HTMLElement {
	manifest?: ManifestDashboard;
}
