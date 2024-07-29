import type { UmbCurrentUserMfaDisableModalElement } from './current-user-mfa-disable-modal.element.js';
import type { Meta, StoryObj } from '@storybook/web-components';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './current-user-mfa-disable-modal.element.js';

const meta: Meta<UmbCurrentUserMfaDisableModalElement> = {
	title: 'Current User/MFA/Disable MFA',
	component: 'umb-current-user-mfa-disable-modal',
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

type Story = StoryObj<UmbCurrentUserMfaDisableModalElement>;

export const Overview: Story = {};
