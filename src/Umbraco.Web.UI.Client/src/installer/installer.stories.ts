import '../core/context/context-provider.element';
import './installer-consent.element';
import './installer-database.element';
import './installer-error.element';
import './installer-installing.element';
import './installer-user.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { rest } from 'msw';

import { UmbInstallerContext } from './installer-context';

import type { UmbInstallerError, UmbInstallerUser } from '.';
import type { UmbracoInstaller } from '../core/models';
export default {
	title: 'Components/Installer/Steps',
	component: 'umb-installer',
	id: 'installer',
	decorators: [
		(story) =>
			html`<umb-context-provider
				style="display: block;margin: 2rem 25%;padding: 1rem;border: 1px solid #ddd;"
				key="umbInstallerContext"
				.value=${new UmbInstallerContext()}>
				${story()}
			</umb-context-provider>`,
	],
} as Meta;

export const Step1User: Story<UmbInstallerUser> = () => html`<umb-installer-user></umb-installer-user>`;
Step1User.storyName = 'Step 1: User';
Step1User.parameters = {
	actions: {
		handles: ['next'],
	},
};

export const Step2Telemetry: Story = () => html`<umb-installer-consent></umb-installer-consent>`;
Step2Telemetry.storyName = 'Step 2: Telemetry data';
Step2Telemetry.parameters = {
	actions: {
		handles: ['previous', 'next'],
	},
};

export const Step3Database: Story = () => html`<umb-installer-database></umb-installer-database>`;
Step3Database.storyName = 'Step 3: Database';
Step3Database.parameters = {
	actions: {
		handles: ['previous', 'submit'],
	},
};

export const Step3DatabasePreconfigured: Story = () => html`<umb-installer-database></umb-installer-database>`;
Step3DatabasePreconfigured.storyName = 'Step 3: Database (preconfigured)';
Step3DatabasePreconfigured.parameters = {
	actions: {
		handles: ['previous', 'submit'],
	},
	msw: {
		handlers: {
			global: null,
			others: [
				rest.get('/umbraco/backoffice/install/settings', (_req, res, ctx) => {
					return res(
						ctx.status(200),
						ctx.json<UmbracoInstaller>({
							user: { consentLevels: [], minCharLength: 2, minNonAlphaNumericLength: 2 },
							databases: [
								{
									id: '1',
									sortOrder: -1,
									displayName: 'SQLite',
									defaultDatabaseName: 'Umbraco',
									providerName: 'Microsoft.Data.SQLite',
									isConfigured: true,
									requiresServer: false,
									serverPlaceholder: null,
									requiresCredentials: false,
									supportsIntegratedAuthentication: false,
									requiresConnectionTest: false,
								},
							],
						})
					);
				}),
			],
		},
	},
};

export const Step4Installing: Story = () => html`<umb-installer-installing></umb-installer-installing>`;
Step4Installing.storyName = 'Step 4: Installing';

export const Step5Error: Story<UmbInstallerError> = ({ error }) =>
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
