import type { UmbMockDataSet } from '../../mock-data-set.types.js';

import { data as dataType } from './data-type.data.js';
import { data as document } from './document.data.js';
import { data as documentType } from './document-type.data.js';
import { data as language } from './language.data.js';
import { data as user } from './user.data.js';
import { data as userGroup } from './user-group.data.js';

export { dataType, document, documentType, language, user, userGroup };

// Type assertion to ensure this module satisfies UmbMockDataSet
const _typeCheck: UmbMockDataSet = {
	dataType,
	document,
	documentType,
	language,
	user,
	userGroup,
};
void _typeCheck;
