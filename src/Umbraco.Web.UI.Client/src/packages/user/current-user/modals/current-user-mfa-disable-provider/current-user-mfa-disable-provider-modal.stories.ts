import type { UmbCurrentUserMfaDisableProviderModalElement } from './current-user-mfa-disable-provider-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './current-user-mfa-disable-provider-modal.element.js';

const meta: Meta<UmbCurrentUserMfaDisableProviderModalElement> = {
	title: 'Current User/MFA/Disable MFA Provider',
	component: 'umb-current-user-mfa-disable-provider-modal',
	decorators: [(Story) => html`<div style="width: 500px; height: 500px;">${Story()}</div>`],
	args: {
		data: {
			providerName: 'SMS',
			displayName: 'SMS',
		},
	},
	parameters: {
		layout: 'centered',
		actions: {
			disabled: true,
		},
	},
};

export default meta;

type Story = StoryObj<UmbCurrentUserMfaDisableProviderModalElement>;

export const Overview: Story = {};
