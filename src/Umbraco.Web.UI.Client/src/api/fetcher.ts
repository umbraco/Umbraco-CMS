import { Fetcher } from 'openapi-typescript-fetch';

import { paths } from '../../schemas/generated-schema';

const fetcher = Fetcher.for<paths>();

fetcher.configure({
    baseUrl: '/umbraco/backoffice'
});

export const getInitStatus = fetcher.path('/init').method('get').create();
export const postUserLogin = fetcher.path('/user/login').method('post').create();
export const postUserLogout = fetcher.path('/user/logout').method('post').create();
export const getInstall = fetcher.path('/install').method('get').create();
export const postInstall = fetcher.path('/install').method('post').create();