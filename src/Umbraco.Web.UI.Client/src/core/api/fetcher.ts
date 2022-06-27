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
export const getInstall = fetcher.path('/install').method('get').create();
export const postInstall = fetcher.path('/install').method('post').create();
