import type { UmbFigureCardElement } from './figure-card.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html, ifDefined } from '@umbraco-cms/backoffice/external/lit';

import './figure-card.element.js';

const meta: Meta<UmbFigureCardElement> = {
	title: 'Generic Components/Figure Card',
	component: 'umb-figure-card',
	args: {
		name: 'Card Name',
		href: 'https://umbraco.com',
		description: 'This is a description of the card.',
		selectable: false,
		selectOnly: false,
		selected: false,
		disabled: false,
		backgroundColor: undefined,
	},
	argTypes: {
		backgroundColor: { control: { type: 'color' } },
	},
	render: (args) =>
		html`<umb-figure-card
			name=${args.name}
			href=${ifDefined(args.href)}
			?selectable=${args.selectable}
			?select-only=${args.selectOnly}
			?selected=${args.selected}
			?disabled=${args.disabled}
			background-color=${args.backgroundColor}
			description=${args.description}>
			<uui-icon name="icon-wand"></uui-icon>
			<uui-button slot="actions" look="secondary" label="Remove">Remove</uui-button>
			<uui-tag slot="tag">Tag</uui-tag>
		</umb-figure-card>`,
};

export default meta;

type Story = StoryObj<UmbFigureCardElement>;

export const Docs: Story = {
	args: {},
};
