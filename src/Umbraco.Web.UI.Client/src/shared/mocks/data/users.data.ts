import { UmbData } from './data';
import { PagedUserResponseModel, UserResponseModel, UserStateModel } from '@umbraco-cms/backoffice/backend-api';

// Temp mocked database
class UmbUsersData extends UmbData<UserResponseModel> {
	constructor(data: UserResponseModel[]) {
		super(data);
	}

	getAll(): PagedUserResponseModel {
		return {
			total: this.data.length,
			items: this.data,
		};
	}

	getById(id: string): UserResponseModel | undefined {
		return this.data.find((user) => user.id === id);
	}

	save(saveItem: UserResponseModel) {
		const foundIndex = this.data.findIndex((item) => item.id === saveItem.id);
		if (foundIndex !== -1) {
			// update
			this.data[foundIndex] = saveItem;
			this.updateData(saveItem);
		} else {
			// new
			this.data.push(saveItem);
		}

		return saveItem;
	}

	protected updateData(updateItem: UserResponseModel) {
		const itemIndex = this.data.findIndex((item) => item.id === updateItem.id);
		const item = this.data[itemIndex];

		console.log('updateData', updateItem, itemIndex, item);

		if (!item) return;

		const itemKeys = Object.keys(item);
		const newItem = {};

		for (const [key] of Object.entries(updateItem)) {
			if (itemKeys.indexOf(key) !== -1) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
				newItem[key] = updateItem[key];
			}
		}

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.data[itemIndex] = newItem;

		console.log('updateData', this.data[itemIndex]);
	}

	// updateUserGroup(ids: string[], userGroup: string) {
	// 	this.data.forEach((user) => {
	// 		if (ids.includes(user.id)) {
	// 		} else {
	// 		}

	// 		this.updateData(user);
	// 	});

	// 	return this.data.map((user) => user.id);
	// }

	// enable(ids: string[]) {
	// 	const users = this.data.filter((user) => ids.includes(user.id));
	// 	users.forEach((user) => {
	// 		user.status = 'enabled';
	// 		this.updateData(user);
	// 	});
	// 	return users.map((user) => user.id);
	// }

	// disable(ids: string[]) {
	// 	const users = this.data.filter((user) => ids.includes(user.id));
	// 	users.forEach((user) => {
	// 		user.status = 'disabled';
	// 		this.updateData(user);
	// 	});
	// 	return users.map((user) => user.id);
	// }
}

export const data: Array<UserResponseModel & { type: string }> = [
	{
		id: 'bca6c733-a63d-4353-a271-9a8b6bcca8bd',
		type: 'user',
		$type: 'UserResponseModel',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Erny Baptista',
		email: 'ebaptista1@csmonitor.com',
		languageIsoCode: 'Kannada',
		state: UserStateModel.ACTIVE,
		lastLoginDate: '9/10/2022',
		lastLockoutDate: '11/23/2021',
		lastPasswordChangeDate: '1/10/2022',
		updateDate: '2/10/2022',
		createDate: '3/13/2022',
		failedLoginAttempts: 946,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: '82e11d3d-b91d-43c9-9071-34d28e62e81d',
		type: 'user',
		$type: 'UserResponseModel',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Amelie Walker',
		email: 'awalker1@domain.com',
		languageIsoCode: 'Japanese',
		state: UserStateModel.INACTIVE,
		lastLoginDate: '4/12/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/1/2023',
		updateDate: '4/12/2023',
		createDate: '4/12/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: 'aa1d83a9-bc7f-47d2-b288-58d8a31f5017',
		type: 'user',
		$type: 'UserResponseModel',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Oliver Kim',
		email: 'okim1@domain.com',
		languageIsoCode: 'Russian',
		state: UserStateModel.ACTIVE,
		lastLoginDate: '4/11/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/5/2023',
		updateDate: '4/11/2023',
		createDate: '4/11/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: 'ff2f4a50-d3d4-4bc4-869d-c7948c160e54',
		type: 'user',
		$type: 'UserResponseModel',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Eliana Nieves',
		email: 'enieves1@domain.com',
		languageIsoCode: 'Spanish',
		state: UserStateModel.INVITED,
		lastLoginDate: '4/10/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/6/2023',
		updateDate: '4/10/2023',
		createDate: '4/10/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
	{
		id: 'c290c6d9-9f12-4838-8567-621b52a178de',
		type: 'user',
		$type: 'UserResponseModel',
		contentStartNodeIds: [],
		mediaStartNodeIds: [],
		name: 'Jasmine Patel',
		email: 'jpatel1@domain.com',
		languageIsoCode: 'Hindi',
		state: UserStateModel.DISABLED,
		lastLoginDate: '4/9/2023',
		lastLockoutDate: '',
		lastPasswordChangeDate: '4/7/2023',
		updateDate: '4/9/2023',
		createDate: '4/9/2023',
		failedLoginAttempts: 0,
		userGroupIds: ['c630d49e-4e7b-42ea-b2bc-edc0edacb6b1'],
	},
];

export const umbUsersData = new UmbUsersData(data);
