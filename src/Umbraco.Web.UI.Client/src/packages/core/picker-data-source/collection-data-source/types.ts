import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbPickerCollectionDataSource extends UmbPickerDataSource, UmbCollectionRepository, UmbApi {}
