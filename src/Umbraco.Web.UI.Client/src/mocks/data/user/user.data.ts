import { dataSet } from '../sets/index.js';
import type {
	UserItemResponseModel,
	UserResponseModel,
	UserTwoFactorProviderModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockUserModel = UserResponseModel & UserItemResponseModel;

export const data: Array<UmbMockUserModel> = dataSet.user;

export const mfaLoginProviders: Array<UserTwoFactorProviderModel> = dataSet.mfaLoginProviders;
