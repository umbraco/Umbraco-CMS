import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_TRANSLATION_SECTION_PATHNAME } from '@umbraco-cms/backoffice/translation';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import type { UmbEntityModel, UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

export const UMB_DICTIONARY_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_TRANSLATION_SECTION_PATHNAME,
	entityType: UMB_DICTIONARY_ENTITY_TYPE,
});

export const UMB_CREATE_DICTIONARY_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbEntityModel['entityType'];
	parentUnique: UmbEntityUnique;
}>('create/parent/:parentEntityType/:parentUnique', UMB_DICTIONARY_WORKSPACE_PATH);

export const UMB_EDIT_DICTIONARY_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	unique: UmbEntityUnique;
}>('edit/:unique', UMB_DICTIONARY_WORKSPACE_PATH);
