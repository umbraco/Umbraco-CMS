import type { ExampleTreeStore } from './tree.store.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const EXAMPLE_TREE_STORE_CONTEXT = new UmbContextToken<ExampleTreeStore>('ExampleTreeStore');
