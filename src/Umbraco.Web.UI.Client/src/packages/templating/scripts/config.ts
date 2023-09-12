import { ScriptResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type ScriptDetails = ScriptResponseModel;

//ENTITY TYPES
export const SCRIPTS_ENTITY_TYPE = 'script';
export const SCRIPTS_ROOT_ENTITY_TYPE = 'script-root';
export const SCRIPTS_FOLDER_ENTITY_TYPE = 'script-folder';
export const SCRIPTS_FOLDER_EMPTY_ENTITY_TYPE = 'script-folder-empty';


export const SCRIPTS_STORE_ALIAS = 'Umb.Store.Scripts';
export const UMB_SCRIPTS_STORE_CONTEXT_TOKEN_ALIAS = 'Umb.Store.Scripts.Context.Token';

export const SCRIPTS_REPOSITORY_ALIAS = 'Umb.Repository.Scripts';

export const SCRIPTS_MENU_ITEM_ALIAS = 'Umb.MenuItem.Scripts';

//TREE
export const SCRIPTS_TREE_ALIAS = 'Umb.Tree.Scripts';
export const SCRIPTS_TREE_ITEM_ALIAS = 'Umb.TreeItem.Scripts';
export const SCRIPTS_TREE_STORE_ALIAS = 'Umb.Store.Scripts.Tree';
export const UMB_SCRIPTS_TREE_STORE_CONTEXT_TOKEN_ALIAS = 'Umb.Store.Scripts.Tree.Context.Token';

//ENTITY (tree) ACTIONS
export const SCRIPTS_ENTITY_ACTION_DELETE_ALIAS = 'Umb.EntityAction.Scripts.Delete';
export const SCRIPTS_ENTITY_ACTION_CREATE_NEW_ALIAS = 'Umb.EntityAction.ScriptsFolder.Create.New';
export const SCRIPTS_ENTITY_ACTION_DELETE_FOLDER_ALIAS = 'Umb.EntityAction.ScriptsFolder.DeleteFolder';
export const SCRIPTS_ENTITY_ACTION_CREATE_FOLDER_NEW_ALIAS = 'Umb.EntityAction.ScriptsFolder.CreateFolder';

//WORKSPACE
export const SCRIPTS_WORKSPACE_ALIAS = 'Umb.Workspace.Scripts';
export const SCRIPTS_WORKSPACE_ACTION_SAVE_ALIAS = 'Umb.WorkspaceAction.Scripts.Save';

