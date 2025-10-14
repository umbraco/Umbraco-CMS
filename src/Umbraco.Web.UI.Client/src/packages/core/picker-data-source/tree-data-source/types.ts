import type { UmbPickerDataSource } from '../data-source/types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';

export interface UmbPickerTreeDataSource extends UmbPickerDataSource, UmbTreeRepository, UmbApi {}
