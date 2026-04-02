import type { UmbUserGroupInputElement } from './user-group-input.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './user-group-input.element.js';

const meta: Meta<UmbUserGroupInputElement> = {
	title: 'Entity/User Group/Components/User Group Input',
	component: 'umb-user-group-input',
	render: () => html`<umb-user-group-input></umb-user-group-input>`,
};

export default meta;
type Story = StoryObj<UmbUserGroupInputElement>;

export const Docs: Story = {
	args: {},
};
