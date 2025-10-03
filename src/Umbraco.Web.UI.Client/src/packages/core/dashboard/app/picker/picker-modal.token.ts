import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDashboardAppPickerModalData {
	multiple?: boolean;
}

export interface UmbDashboardAppPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_DASHBOARD_APP_PICKER_MODAL = new UmbModalToken<
	UmbDashboardAppPickerModalData,
	UmbDashboardAppPickerModalValue
>('Umb.Modal.DashboardAppPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
