import type { UmbDashboardAppPickerContext } from './picker.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_DASHBOARD_APP_PICKER_CONTEXT = new UmbContextToken<UmbDashboardAppPickerContext>(
	'Umb.Modal.DashboardAppPicker',
);
