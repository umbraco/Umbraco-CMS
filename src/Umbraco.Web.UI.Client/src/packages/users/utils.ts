import { InterfaceColor, InterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

interface DisplayStatus {
	look: InterfaceLook; 
	color: InterfaceColor;
	key: string;
}
const userStates: DisplayStatus[] = [
	{ "key": "All", color:"positive", look: "secondary" } ,
	{ "key": "Active", "color": "positive", look: "primary" },
	{ "key": "Disabled", "color": "danger", look: "primary" },
	{ "key": "LockedOut", "color": "danger", look: "secondary" },
	{ "key": "Invited", "color": "warning", look: "primary" },
	{ "key": "Inactive", "color": "warning", look: "secondary" }
];

export const getDisplayStateFromUserStatus = (status?: UserStateModel): DisplayStatus => 
	userStates
		.filter(state => state.key === status)!
		.map(state => ({
			...state,
			key:'state'+state.key
		}))[0]
