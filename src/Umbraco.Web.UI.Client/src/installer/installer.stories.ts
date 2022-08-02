import './installer.element';

import { Meta, Story } from '@storybook/web-components';
import { css, CSSResult, LitElement } from 'lit';
import { html } from 'lit-html';
import { customElement } from 'lit/decorators.js';
import { BehaviorSubject } from 'rxjs';

import { UmbInstallerUser } from '.';
import { UmbContextProviderMixin } from '../core/context';
import { PostInstallRequest, UmbracoInstaller } from '../core/models';

@customElement('mock-installer-context')
class UmbInstallerContext extends UmbContextProviderMixin(LitElement) {
	static styles: CSSResult[] = [
		css`
			:host {
				display: block;
				margin: 2rem 25%;
				padding: 1rem;
				border: 1px solid #ddd;
			}
		`,
	];

	constructor() {
		super();
		const data = new BehaviorSubject<PostInstallRequest>({
			telemetryLevel: 'Basic',
			user: {
				name: 'Umbraco',
				email: 'test@umbraco.com',
				password: 'test123456',
				subscribeToNewsletter: false,
			},
		});

		this.provideContext('umbInstallerContext', {
			data,
			getData: () => data.value,
			settings: new BehaviorSubject<UmbracoInstaller>({
				user: {
					consentLevels: [{ description: 'This is a consent text', level: 'Basic' }],
					minCharLength: 2,
					minNonAlphaNumericLength: 2,
				},
				databases: [
					{
						id: '123',
						defaultDatabaseName: 'umbraco',
						displayName: 'SQLite',
						isConfigured: false,
						providerName: 'Umbraco',
						requiresConnectionTest: false,
						requiresCredentials: false,
						requiresServer: false,
						serverPlaceholder: 'umbraco',
						sortOrder: 0,
						supportsIntegratedAuthentication: false,
					},
				],
			}),
			appendData: () => true,
			requestInstall: () => Promise.resolve(),
		} as Partial<UmbInstallerContext>);
	}

	render() {
		return html`<slot></slot>`;
	}
}

export default {
	title: 'Components/Installer/Steps',
	component: 'umb-installer',
	id: 'installer',
	decorators: [(story) => html`<mock-installer-context>${story()}</mock-installer-context>`],
} as Meta;

export const Step1User: Story<UmbInstallerUser> = () => html`<umb-installer-user></umb-installer-user>`;
Step1User.storyName = 'Step 1: User';
Step1User.parameters = {
	actions: {
		handles: ['next'],
	},
};

export const Step2Database: Story = () => html`<umb-installer-database></umb-installer-database>`;
Step2Database.storyName = 'Step 2: Database';
Step2Database.parameters = {
	actions: {
		handles: ['previous', 'next'],
	},
};

export const Step3Installing: Story = () => html`<umb-installer-installing></umb-installer-installing>`;
Step3Installing.storyName = 'Step 3: Installing';
