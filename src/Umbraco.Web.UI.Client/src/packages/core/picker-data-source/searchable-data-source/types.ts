import type { UmbPickerDataSource } from '../types.js';
import type { UmbSearchRepository } from '@umbraco-cms/backoffice/search';

export interface UmbPickerSearchableDataSource extends UmbPickerDataSource, UmbSearchRepository<any> {}
