import type { UmbCardElement } from './card.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

import './card.element.js';

const meta: Meta<UmbCardElement> = {
	title: 'Generic Components/Card',
	component: 'umb-card',
	args: {
		name: 'Card Name',
		href: 'https://umbraco.com',
		description: 'This is a description of the card.',
		selectable: false,
		selectOnly: false,
		selected: false,
		disabled: false,
		background: undefined,
	},
	argTypes: {
		background: { control: { type: 'color' } },
	},
	render: (args) =>
		html`<umb-card
			name=${args.name}
			href=${ifDefined(args.href)}
			?selectable=${args.selectable}
			?select-only=${args.selectOnly}
			?selected=${args.selected}
			?disabled=${args.disabled}
			background=${args.background}
			description=${args.description}>
			<uui-icon name="icon-wand"></uui-icon>
			<uui-button slot="actions" look="secondary" label="Remove">Remove</uui-button>
			<uui-tag slot="tag">Tag</uui-tag>
		</umb-card>`,
};

export default meta;

type Story = StoryObj<UmbCardElement>;

export const Docs: Story = {
	args: {},
};
