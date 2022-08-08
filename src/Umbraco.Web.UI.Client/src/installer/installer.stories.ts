import '../core/context/context-provider.element';
import './installer-consent.element';
import './installer-database.element';
import './installer-installing.element';
import './installer-user.element';

import { Meta, Story } from '@storybook/web-components';
import { html } from 'lit-html';
import { rest } from 'msw';

import { UmbInstallerUser } from '.';
import { UmbracoInstaller } from '../core/models';
import { UmbInstallerContext } from './installer-context';

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
		handles: ['previous', 'next'],
	},
};

export const Step3DatabasePreconfigured: Story = () => html`<umb-installer-database></umb-installer-database>`;
Step3DatabasePreconfigured.storyName = 'Step 3: Database (preconfigured)';
Step3DatabasePreconfigured.parameters = {
	actions: {
		handles: ['previous', 'next'],
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
