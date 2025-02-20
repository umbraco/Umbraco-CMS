import { installerContextProvider } from '../shared/utils.story-helpers.js';
import { UmbInstallerContext } from '../installer.context.js';
import type { UmbInstallerErrorElement } from './installer-error.element.js';
import type { Meta, StoryFn } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './installer-error.element.js';

const error = {
	type: 'validation',
	status: 400,
	detail: 'The form did not pass validation',
	title: 'Validation error',
	errors: {
		'user.password': [
			'The password must be at least 6 characters long',
			'The password must contain at least one number',
		],
		databaseName: ['The database name is required'],
	},
};

const installerContext = new UmbInstallerContext();
installerContext.setInstallStatus(error);

export default {
	title: 'Apps/Installer/Steps',
	component: 'umb-installer-error',
	id: 'umb-installer-error',
	decorators: [(story) => installerContextProvider(story, installerContext)],
} as Meta;

export const Step5Error: StoryFn<UmbInstallerErrorElement> = () => html`<umb-installer-error></umb-installer-error>`;
Step5Error.storyName = 'Step 5: Error';
