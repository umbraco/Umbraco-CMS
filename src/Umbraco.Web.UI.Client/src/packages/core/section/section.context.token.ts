import type { UmbSectionContext } from './section.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SECTION_CONTEXT = new UmbContextToken<UmbSectionContext>('UmbSectionContext');
