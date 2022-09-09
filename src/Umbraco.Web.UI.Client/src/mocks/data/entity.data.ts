import { UmbData } from './data';
import { Entity, entities } from './entities';

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
