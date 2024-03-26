import type { Meta, StoryObj } from '@storybook/web-components';
import type { UmbMfaProviderDefaultElement } from './mfa-provider-default.element.js';

import './mfa-provider-default.element.js';

const meta: Meta<UmbMfaProviderDefaultElement> = {
	id: 'umb-current-user-mfa-provider-modal',
	title: 'Current User/Modals/MFA Provider Modal',
	component: 'umb-mfa-provider-default',
	args: {
		providerName: 'SMS',
		isEnabled: true,
	},
};

export default meta;

type Story = StoryObj<UmbMfaProviderDefaultElement>;

export const Overview: Story = {};
