import { UmbData } from './data';
import { entities } from './entities';

export interface Entity {
	key: string;
	name: string;
	icon: string;
	type: string;
	hasChildren: boolean;
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
