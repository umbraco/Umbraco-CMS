import { StylesheetResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbStylesheetDetailModel extends StylesheetResponseModel {}

export * from './repository/index.js';
export { UmbStylesheetTreeRepository } from './tree/index.js';
