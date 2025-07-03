import type { UmbDropdownElement } from './dropdown.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './dropdown.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

const meta: Meta<UmbDropdownElement> = {
	title: 'Generic Components/Dropdown',
	component: 'umb-dropdown',
	render: () =>
		html` <umb-dropdown label="Select">
			<span slot="label">Select</span>
			<div>Dropdown Content</div>
		</umb-dropdown>`,
};

export default meta;
type Story = StoryObj<UmbDropdownElement>;

export const Overview: Story = {
	args: {},
};
