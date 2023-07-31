import { StylesheetResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: temp until we have a proper stylesheet model
export type StylesheetDetails = StylesheetResponseModel;

export const STYLESHEET_ENTITY_TYPE = 'stylesheet';
export * from './repository/index.js';
