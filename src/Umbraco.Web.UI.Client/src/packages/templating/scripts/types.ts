import { UmbScriptEntityType } from './entity.js';

export interface UmbScriptDetailModel {
	entityType: UmbScriptEntityType;
	unique: string;
	parentUnique: string | null;
	path: string;
	name: string;
	content: string;
}
