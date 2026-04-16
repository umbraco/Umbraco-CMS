import type { UmbMockDataSet } from '../../mock-data-set.types.js';

import { data as webhook } from './webhook.data.js';
import { data as user } from './user.data.js';
import { data as userGroup } from './user-group.data.js';
import { data as language } from './language.data.js';

export { webhook, user, userGroup, language };

// Type assertion to ensure this module satisfies UmbMockDataSet
const _typeCheck: UmbMockDataSet = {
	webhook,
	user,
	userGroup,
	language,
};
void _typeCheck;
