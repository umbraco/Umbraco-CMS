import type { UmbMockDataSet } from '../../mock-data-set.types.js';
import { DataTypeChangeModeModel } from '@umbraco-cms/backoffice/external/backend-api';

import { data as dataType } from './data-type.data.js';
import { data as document } from './document.data.js';
import { data as documentType } from './document-type.data.js';
import { data as language } from './language.data.js';
import { data as user } from './user.data.js';
import { data as userGroup } from './user-group.data.js';

export { dataType, document, documentType, language, user, userGroup };

export const documentTypeConfiguration = {
	dataTypesCanBeChanged: DataTypeChangeModeModel.TRUE,
	disableTemplates: false,
	useSegments: true,
	reservedFieldNames: [],
};

// Type assertion to ensure this module satisfies UmbMockDataSet
({
	dataType,
	document,
	documentType,
	documentTypeConfiguration,
	language,
	user,
	userGroup,
} satisfies UmbMockDataSet);
