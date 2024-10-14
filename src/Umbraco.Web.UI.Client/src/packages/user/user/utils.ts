import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import type { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbUserDisplayStatus {
	look: UUIInterfaceLook;
	color: UUIInterfaceColor;
	key: string;
}

const userStates: UmbUserDisplayStatus[] = [
	{ key: 'All', color: 'positive', look: 'secondary' },
	{ key: 'Active', color: 'positive', look: 'primary' },
	{ key: 'Disabled', color: 'danger', look: 'primary' },
	{ key: 'LockedOut', color: 'danger', look: 'secondary' },
	{ key: 'Invited', color: 'warning', look: 'primary' },
	{ key: 'Inactive', color: 'warning', look: 'primary' },
];

export const getDisplayStateFromUserStatus = (status: UserStateModel): UmbUserDisplayStatus =>
	userStates
		.filter((state) => state.key === status)
		.map((state) => ({
			...state,
			key: 'state' + state.key,
		}))[0];

export const TimeFormatOptions: Intl.DateTimeFormatOptions = { dateStyle: 'long', timeStyle: 'short' };
