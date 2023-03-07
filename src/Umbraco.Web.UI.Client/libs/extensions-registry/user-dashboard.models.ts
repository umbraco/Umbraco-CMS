import type { ManifestElement } from './models';

/**
 * A user dashboard extension is shown in the dialog that opens when the user clicks on the user avatar in the top right corner
 */
export interface ManifestUserDashboard extends ManifestElement {
	type: 'userDashboard';
}

