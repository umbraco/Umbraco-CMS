import type { UmbScriptEntityType } from './entity.js';

export interface UmbScriptDetailModel {
	entityType: UmbScriptEntityType;
	unique: string;
	path: string;
	name: string;
	content: string;
}
