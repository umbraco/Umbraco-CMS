import { InterfaceColor, InterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

export const getLookAndColorFromUserStatus = (
	status?: UserStateModel
): { look: InterfaceLook; color: InterfaceColor } => {
	switch (status) {
		case UserStateModel.INACTIVE:
		case UserStateModel.INVITED:
			return { look: 'primary', color: 'warning' };
		case UserStateModel.ACTIVE:
			return { look: 'primary', color: 'positive' };
		case UserStateModel.DISABLED:
			return { look: 'primary', color: 'danger' };
		default:
			return { look: 'secondary', color: 'default' };
	}
};
