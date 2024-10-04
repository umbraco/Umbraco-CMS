import type { PathMatch } from './model.js';

export const CATCH_ALL_WILDCARD: string = '**';
export const TRAVERSE_FLAG: string = '\\.\\.\\/';
export const PARAM_IDENTIFIER: RegExp = /:([^\\/]+)/g;
export const ROUTER_SLOT_TAG_NAME: string = 'router-slot';
export const GLOBAL_ROUTER_EVENTS_TARGET = window;
export const HISTORY_PATCH_NATIVE_KEY: string = `native`;
export const DEFAULT_PATH_MATCH: PathMatch = 'prefix';
