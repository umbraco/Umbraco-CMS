import type { UmbElementFolderEntityType } from './tree/folder/entity.js';

export const UMB_ELEMENT_ENTITY_TYPE = 'element';
export const UMB_ELEMENT_ROOT_ENTITY_TYPE = 'element-root';

export type UmbElementEntityType = typeof UMB_ELEMENT_ENTITY_TYPE;
export type UmbElementRootEntityType = typeof UMB_ELEMENT_ROOT_ENTITY_TYPE;
export type UmbElementEntityTypeUnion = UmbElementEntityType | UmbElementRootEntityType | UmbElementFolderEntityType;
