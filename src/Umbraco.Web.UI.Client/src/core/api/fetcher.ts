import { Fetcher } from 'openapi-typescript-fetch';

import { paths } from '../../../schemas/generated-schema';

const fetcher = Fetcher.for<paths>();

fetcher.configure({
	baseUrl: '/umbraco/backoffice',
});

export const getServerStatus = fetcher.path('/server/status').method('get').create();
export const getServerVersion = fetcher.path('/server/version').method('get').create();
export const getUser = fetcher.path('/user').method('get').create();
export const postUserLogin = fetcher.path('/user/login').method('post').create();
export const postUserLogout = fetcher.path('/user/logout').method('post').create();
export const getUserSections = fetcher.path('/user/sections').method('get').create();
export const getInstallSettings = fetcher.path('/install/settings').method('get').create();
export const postInstallValidateDatabase = fetcher.path('/install/validateDatabase').method('post').create();
export const postInstallSetup = fetcher.path('/install/setup').method('post').create();
export const getUpgradeSettings = fetcher.path('/upgrade/settings').method('get').create();
export const PostUpgradeAuthorize = fetcher.path('/upgrade/authorize').method('post').create();
export const getManifests = fetcher.path('/manifests').method('get').create();
