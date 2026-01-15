import type { UmbInputMultipleTextStringElement } from './input-multiple-text-string.element.js';
import type { Meta, StoryObj } from '@storybook/web-components-vite';
import { html } from '@umbraco-cms/backoffice/external/lit';

import './input-multiple-text-string.element.js';

const meta: Meta<UmbInputMultipleTextStringElement> = {
	title: 'Generic Components/Inputs/Multiple Text String',
	component: 'umb-input-multiple-text-string',
	render: (args) =>
		html`<umb-input-multiple-text-string
			.min="${args.min}"
			.max="${args.max}"
			.minMessage="${args.minMessage}"
			.maxMessage="${args.maxMessage}"></umb-input-multiple-text-string>`,
};

export default meta;
type Story = StoryObj<UmbInputMultipleTextStringElement>;

export const Docs: Story = {
	args: {
		min: 0,
		max: 10,
		minMessage: 'This field needs more items',
		maxMessage: 'This field has too many items',
	},
};
