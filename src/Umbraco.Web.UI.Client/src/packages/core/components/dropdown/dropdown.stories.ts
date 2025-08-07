import type { UmbDropdownElement } from './dropdown.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import './dropdown.element.js';
import { html } from '@umbraco-cms/backoffice/external/lit';

const meta: Meta<UmbDropdownElement> = {
	title: 'Generic Components/Dropdown',
	component: 'umb-dropdown',
	render: (args) =>
		html` <umb-dropdown
			.open=${args.open}
			.label=${args.label}
			.look=${args.look}
			.color=${args.color}
			.placement=${args.placement}
			.compact=${args.compact}
			?hide-expand=${args.hideExpand}>
			<span slot="label">${args.label}</span>
			<div>Dropdown Content</div>
		</umb-dropdown>`,
};

export default meta;
type Story = StoryObj<UmbDropdownElement>;

export const Docs: Story = {
	args: {
		open: false,
		label: 'Dropdown',
		look: 'default',
		color: 'default',
		placement: 'bottom-start',
		compact: false,
		hideExpand: false,
	},
};
