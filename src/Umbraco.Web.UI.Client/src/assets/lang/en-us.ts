/**
 * Creator Name: The Umbraco community
 * Creator Link: https://docs.umbraco.com/umbraco-cms/extending/language-files
 *
 * Language Alias: en
 * Language Int Name: English (US)
 * Language Local Name: English (US)
 * Language LCID:
 * Language Culture: en-US
 */
import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
	analytics: {
		analyticsDescription:
			'In order to improve Umbraco and add new functionality based on as relevant information as possible, we would like to collect system- and usage information from your installation.<br>Aggregate data will be shared on a regular basis as well as learnings from these metrics.<br>Hopefully, you will help us collect some valuable data.<br><br>We <strong>WILL NOT</strong> collect any personal data such as content, code, user information, and all data will be fully anonymized.',
		basicLevelDescription: 'We will send an anonymized site ID, Umbraco version, and packages installed',
		detailedLevelDescription:
			'We will send: <ul>li>Anonymized site ID, Umbraco version, and packages installed.</li><li>Number of: Root nodes, Content nodes, Media, Document Types, Templates, Languages, Domains, User Group, Users, Members, Backoffice external login providers, and Property Editors in use.</li><li>System information: Webserver, server OS, server framework, server OS language, and database provider.</li><li>Configuration settings: ModelsBuilder mode, if custom Umbraco path exists, ASP environment, whether the delivery API is enabled, and allows public access, and if you are in debug mode.</li></ul><em>We might change what we send on the Detailed level in the future. If so, it will be listed above.<br>By choosing "Detailed" you agree to current and future anonymized information being collected.</em>',
		minimalLevelDescription: 'We will only send an anonymized site ID to let us know that the site exists.',
	},
	blockEditor: {
		labelBackgroundColor: 'Background color',
		labelIconColor: 'Icon color',
	},
	buttons: {
		justifyCenter: 'Center',
	},
	colorpicker: {
		noColors: 'You have not configured any approved colors',
	},
	colorPickerConfigurations: {
		colorsDescription: 'Add, remove or sort colors',
		colorsTitle: 'Colors',
		showLabelDescription:
			'Stores colors as a JSON object containing both the color hex string and label, rather than just the hex string.',
	},
	create: {
		folderDescription: 'Used to organize items and other folders. Keep items structured and easy to access.',
	},
	errors: {
		externalLoginFailed:
			'The server failed to authorize you against the external login provider. Please close the window and try again.',
		unauthorized: 'You were not authorized before performing this action',
	},
	graphicheadline: {
		backgroundcolor: 'Background color',
		color: 'Text color',
	},
	installer: {
		databaseNotFound:
			'<p>Database not found! Please check that the information in the "connection string" of the "web.config" file is correct.</p><p>To proceed, please edit the "web.config" file (using Visual Studio or your favorite text editor), scroll to the bottom, add the connection string for your database in the key named "UmbracoDbDSN" and save the file.</p><p>Click the <strong>retry</strong> button when done.<br /><a href="https://our.umbraco.com/documentation/Reference/Config/webconfig/" target="_blank" rel="noopener">More information on editing web.config here</a>.</p>',
		showLabelDescription:
			'Stores colors as a JSON object containing both the color hex string and label, rather than just the hex string.',
	},
	openidErrors: {
		unauthorizedClient: 'Unauthorized client',
	},
	user: {
		userinviteAvatarMessage:
			'Uploading a photo of yourself will make it easy for other users to recognize you. Click the circle above to upload your photo.',
	},
} as UmbLocalizationDictionary;
