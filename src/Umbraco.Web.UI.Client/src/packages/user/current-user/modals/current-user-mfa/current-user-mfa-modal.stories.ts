import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbCurrentUserMfaModalElement } from './current-user-mfa-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './current-user-mfa-modal.element.js';

const meta: Meta<UmbCurrentUserMfaModalElement> = {
	title: 'Current User/MFA/Configure MFA Providers',
	component: 'umb-current-user-mfa-modal',
	decorators: [(Story) => html`<div style="width: 500px; height: 500px;">${Story()}</div>`],
	parameters: {
		layout: 'centered',
		actions: {
			disabled: true,
		},
	},
};

export default meta;

type Story = StoryObj<UmbCurrentUserMfaModalElement>;

export const Overview: Story = {};
