import { UmbScriptEntityType } from './entity.js';

export interface UmbScriptDetailModel {
	entityType: UmbScriptEntityType;
	path: string; // TODO: change to unique when mapping is done
	name: string;
	content: string;
}
