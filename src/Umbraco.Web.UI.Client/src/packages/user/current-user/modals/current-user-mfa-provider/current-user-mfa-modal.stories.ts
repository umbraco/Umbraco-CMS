import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbMfaProviderDefaultElement } from '../../components/mfa-provider-default.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

import '../../components/mfa-provider-default.element.js';

const meta: Meta<UmbMfaProviderDefaultElement> = {
	id: 'umb-current-user-mfa-provider-modal',
	title: 'Current User/Modals/MFA Provider Modal',
	component: 'umb-mfa-provider-default',
	decorators: [(Story) => html`<div style="width: 500px; height: 500px;">${Story()}</div>`],
	args: {
		providerName: 'SMS',
		isEnabled: true,
	},
	parameters: {
		layout: 'centered',
	},
};

export default meta;

type Story = StoryObj<UmbMfaProviderDefaultElement>;

export const Overview: Story = {};
