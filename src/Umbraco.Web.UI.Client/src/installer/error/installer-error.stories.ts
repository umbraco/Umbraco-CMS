import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';

import type { UmbInstallerErrorElement } from './installer-error.element';
import { installerContextProvider } from '../shared/utils.story-helpers';
import './installer-error.element';

export default {
	title: 'Components/Installer/Steps',
	component: 'umb-installer-error',
	id: 'umb-installer-error',
	decorators: [installerContextProvider],
} as Meta;

export const Step5Error: Story<UmbInstallerErrorElement> = ({ error }) =>
	html`<umb-installer-error .error=${error}></umb-installer-error>`;
Step5Error.storyName = 'Step 5: Error';
Step5Error.args = {
	error: {
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
	},
};
Step5Error.parameters = {
	actions: {
		handles: ['reset'],
	},
};
