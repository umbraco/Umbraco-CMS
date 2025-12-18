import { data } from '../data/object-type.data.js';
import { UmbMockDBBase } from './utils/mock-db-base.js';
import type { ObjectTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

class UmbObjectTypeMockDB extends UmbMockDBBase<ObjectTypeResponseModel> {
	constructor() {
		super(data);
	}

	getById(id: string) {
		return this.data.find((item) => item.id === id);
	}
}

export const umbObjectTypeMockDb = new UmbObjectTypeMockDB();
