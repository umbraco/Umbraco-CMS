import type { UmbMockUserGroupModel } from '../../mock-data-set.types.js';
import { DANISH_ISO_CODE, ENGLISH_ISO_CODE } from './language.data.js';

/**
 * A user group exists for every combination of the languages the user may edit and
 * whether the user may edit shared data (data that does not vary by language).
 */
interface UmbLanguageAccessScenario {
	/** Used in ids and aliases, for example "danish-only" */
	key: string;
	/** Shown to the person clicking through, for example "Danish only" */
	label: string;
	languages: Array<string>;
	hasAccessToAllLanguages: boolean;
}

export const LANGUAGE_ACCESS_SCENARIOS: Array<UmbLanguageAccessScenario> = [
	{ key: 'all-languages', label: 'All languages', languages: [], hasAccessToAllLanguages: true },
	{
		key: 'english-only',
		label: 'English only (default language)',
		languages: [ENGLISH_ISO_CODE],
		hasAccessToAllLanguages: false,
	},
	{ key: 'danish-only', label: 'Danish only', languages: [DANISH_ISO_CODE], hasAccessToAllLanguages: false },
	{
		key: 'english-and-danish',
		label: 'English and Danish',
		languages: [ENGLISH_ISO_CODE, DANISH_ISO_CODE],
		hasAccessToAllLanguages: false,
	},
	{ key: 'no-languages', label: 'No languages', languages: [], hasAccessToAllLanguages: false },
];

export const userGroupId = (scenario: UmbLanguageAccessScenario, canEditSharedData: boolean) =>
	`document-language-permissions-user-group-${scenario.key}-${canEditSharedData ? 'can' : 'cannot'}-edit-shared-data-id`;

export const userGroupName = (scenario: UmbLanguageAccessScenario, canEditSharedData: boolean) =>
	`${scenario.label}, ${canEditSharedData ? 'can' : 'cannot'} edit shared data`;

/**
 *
 * @param scenario
 * @param canEditSharedData
 */
function createUserGroup(scenario: UmbLanguageAccessScenario, canEditSharedData: boolean): UmbMockUserGroupModel {
	const name = userGroupName(scenario, canEditSharedData);
	return {
		id: userGroupId(scenario, canEditSharedData),
		name,
		alias: `documentLanguagePermissions-${scenario.key}-${canEditSharedData ? 'can' : 'cannot'}-edit-shared-data`,
		description: `Languages: ${scenario.label}. Shared data: ${canEditSharedData ? 'can' : 'cannot'} be edited.`,
		icon: 'icon-users',
		fallbackPermissions: [
			'Umb.Document.Read',
			'Umb.Document.Update',
			'Umb.Document.Publish',
			'Umb.Document.Unpublish',
			'Umb.Document.PropertyValue.Read',
			'Umb.Document.PropertyValue.Write',
		],
		permissions: [],
		sections: ['Umb.Section.Content'],
		languages: scenario.languages,
		hasAccessToAllLanguages: scenario.hasAccessToAllLanguages,
		hasAccessToInvariantForVariant: canEditSharedData,
		documentRootAccess: true,
		elementRootAccess: true,
		mediaRootAccess: false,
		aliasCanBeChanged: false,
		isDeletable: false,
		flags: [],
	};
}

export const data: Array<UmbMockUserGroupModel> = LANGUAGE_ACCESS_SCENARIOS.flatMap((scenario) => [
	createUserGroup(scenario, true),
	createUserGroup(scenario, false),
]);
