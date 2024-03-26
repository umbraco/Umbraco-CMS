import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbCurrentUserMfaModalElement } from './current-user-mfa-modal.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './current-user-mfa-modal.element.js';

const meta: Meta<UmbCurrentUserMfaModalElement> = {
	id: 'umb-current-user-mfa-modal',
	title: 'Current User/Modals/MFA Modal',
	component: 'umb-current-user-mfa-modal',
	decorators: [(Story) => html`<div style="width: 500px; height: 500px;">${Story()}</div>`],
	parameters: {
		layout: 'centered',
	},
};

export default meta;

type Story = StoryObj<UmbCurrentUserMfaModalElement>;

export const Overview: Story = {};
