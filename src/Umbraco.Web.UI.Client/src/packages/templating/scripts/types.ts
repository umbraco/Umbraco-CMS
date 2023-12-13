import { UmbScriptEntityType } from './entity.js';

export interface UmbScriptDetailModel {
	entityType: UmbScriptEntityType;
	unique: string;
	parentUnique: string | null;
	name: string;
	content: string;
}
