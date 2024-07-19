import { UmbPathPattern } from '../router/path-pattern.class.js';

export const UMB_SECTION_PATH_PATTERN = new UmbPathPattern<{ sectionName: string }>('section/:sectionName');
