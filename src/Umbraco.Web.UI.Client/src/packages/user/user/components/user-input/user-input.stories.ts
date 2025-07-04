import type { UmbUserInputElement } from './user-input.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './user-input.element.js';

const meta: Meta<UmbUserInputElement> = {
	title: 'Entity/User/Components/Input User',
	component: 'umb-user-input',
	render: () => html`<umb-user-input></umb-user-input>`,
};

export default meta;
type Story = StoryObj<UmbUserInputElement>;

export const Docs: Story = {
	args: {},
};
