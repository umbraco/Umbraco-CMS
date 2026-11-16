import type { UmbMockDataSet } from '../../mock-data-set.types.js';

import { data as baseDataType } from './data-type.data.js';
import { data as baseDocument } from './document.data.js';
import { data as baseDocumentType } from './document-type.data.js';
import { data as baseElement } from './element.data.js';
import { data as baseElementType } from './element-type.data.js';
import { data as language } from './language.data.js';
import {
	dataTypes as nestedBlocksDataTypes,
	documents as nestedBlocksDocuments,
	documentTypes as nestedBlocksDocumentTypes,
	elements as nestedBlocksElements,
	elementTypes as nestedBlocksElementTypes,
} from './nested-blocks.data.js';
import { data as user } from './user.data.js';
import { data as userGroup } from './user-group.data.js';

const dataType = [...baseDataType, ...nestedBlocksDataTypes];
const document = [...baseDocument, ...nestedBlocksDocuments];
const element = [...baseElement, ...nestedBlocksElements];
// Element types are document types with `isElement`
const documentType = [
	...baseDocumentType,
	...nestedBlocksDocumentTypes,
	...baseElementType,
	...nestedBlocksElementTypes,
];

export { dataType, document, documentType, element, language, user, userGroup };

// Type assertion to ensure this module satisfies UmbMockDataSet
({
	dataType,
	document,
	documentType,
	element,
	language,
	user,
	userGroup,
}) satisfies UmbMockDataSet;
