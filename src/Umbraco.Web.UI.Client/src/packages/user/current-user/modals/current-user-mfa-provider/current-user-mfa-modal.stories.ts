import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbCurrentUserMfaProviderModalElement } from './current-user-mfa-provider-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './current-user-mfa-provider-modal.element.js';

const meta: Meta<UmbCurrentUserMfaProviderModalElement> = {
	id: 'umb-current-user-mfa-provider-modal',
	title: 'Current User/Modals/MFA Provider Modal',
	component: 'umb-current-user-mfa-provider-modal',
	decorators: [(Story) => html`<div style="width: 500px; height: 500px;">${Story()}</div>`],
	args: {
		data: {
			providerName: 'SMS',
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

type Story = StoryObj<UmbCurrentUserMfaProviderModalElement>;

export const Overview: Story = {};
