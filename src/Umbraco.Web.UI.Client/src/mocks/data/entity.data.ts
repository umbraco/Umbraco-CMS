import { UmbData } from './data';
import { entities } from './entities';

export interface Entity {
	id: number;
	key: string;
	name: string;
	icon?: string; // TODO: Should this be here?
	type: string;
	hasChildren: boolean; // TODO: Should this be here?
	parentKey: string;
	isTrashed: boolean;
}

// Temp mocked database
class UmbEntityData extends UmbData<Entity> {
	constructor() {
		super(entities);
	}

	getItems(type = '', parentKey = '') {
		if (!type) return [];
		return entities.filter((item) => item.type === type && item.parentKey === parentKey);
	}
}

export const umbEntityData = new UmbEntityData();
