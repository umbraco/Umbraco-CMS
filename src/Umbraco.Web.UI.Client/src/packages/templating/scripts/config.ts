import { ScriptResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type ScriptDetails = ScriptResponseModel;

export const SCRIPTS_ENTITY_TYPE = 'script';
export const SCRIPTS_ROOT_ENTITY_TYPE = 'script-root';
export const SCRIPTS_FOLDER_ENTITY_TYPE = 'script-folder';
export const SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE = 'script-folder-empty';

export const SCRIPTS_REPOSITORY_ALIAS = 'Umb.Repository.Scripts';

export const SCRIPTS_TREE_ALIAS = 'Umb.Tree.Scripts';

export const UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN_ALIAS = 'Umb.Store.Scripts.Tree';
export const UMB_SCRIPTS_STORE_CONTEXT_TOKEN_ALIAS = 'Umb.Store.Scripts';
