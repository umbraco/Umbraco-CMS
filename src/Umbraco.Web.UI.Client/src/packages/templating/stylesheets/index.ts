import { StylesheetResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type StylesheetDetails = StylesheetResponseModel;

export * from './repository/index.js';
export { UmbStylesheetTreeRepository } from './tree/index.js';
