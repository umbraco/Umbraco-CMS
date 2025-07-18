import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDashboardAppPickerContext } from './picker.context';

export const UMB_DASHBOARD_APP_PICKER_CONTEXT = new UmbContextToken<UmbDashboardAppPickerContext>(
	'Umb.Modal.DashboardAppPicker',
);
